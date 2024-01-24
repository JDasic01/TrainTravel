using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using API.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Controllers
{
    [ApiController]
    [Route("csv-file-upload")]
    public class CSVFileController : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly ILogger<CSVFileController> _logger;

        public CSVFileController(ILogger<CSVFileController> logger, IGraphClient client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpPost("upload-cities", Name = "UploadCitiesCSV")]
        public async Task<IActionResult> CreateCities(IFormFile formFile)
        {
            try
            {
                using (var reader = new StreamReader(formFile.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var cityRecords = csv.GetRecords<CityCSVModel>();
                    foreach (var cityRecord in cityRecords)
                    {
                        var city = new City
                        {
                            city_id = cityRecord.city_id,
                            city_name = cityRecord.city_name,
                            city_routes = null, // new HashSet<CityRoute>(),
                            city_to_city = null // new HashSet<CityToCity>()
                        };
                        await _client.Cypher.Create("(c:City $city)")
                                            .WithParam("city", city)
                                            .ExecuteWithoutResultsAsync();
                    }  
                }

                return Ok("CSV file for cities uploaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cities.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("upload-routes", Name = "UploadRoutesCSV")]
        public async Task<IActionResult> CreateRoutes(IFormFile formFile)
        {
            try
            {
                using (var reader = new StreamReader(formFile.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var routeRecords = csv.GetRecords<RouteCSVModel>();
                    foreach (var routeRecord in routeRecords)
                    {
                        var route = new API.Models.Route
                        {
                            route_id = routeRecord.route_id,
                            mileage = routeRecord.mileage,
                            start_city_id = routeRecord.start_city_id,
                            end_city_id = routeRecord.end_city_id,
                            city_routes = null
                        };
                        
                        await _client.Cypher.Create("(r:Route $route)")
                                            .WithParam("route", route)
                                            .ExecuteWithoutResultsAsync();

                        await _client.Cypher.Match("(city:City), (route:Route)")
                                            .Where((City c) => c.city_id == routeRecord.start_city_id)
                                            .AndWhere((API.Models.Route r) => r.route_id == routeRecord.route_id)
                                            .Create("city-[:HAS_ROUTE]->route")
                                            .ExecuteWithoutResultsAsync();

                        // start end city relation
                        //var start_city = (await _client.Cypher.Match("(c:City)")
                        //                        .Where((City c) => c.city_id == routeRecord.start_city_id)
                        //                        .Return(c => c.As<City>()).ResultsAsync)
                        //                        .SingleOrDefault();

                        //var end_city = (await _client.Cypher.Match("(c:City)")
                        //                    .Where((City c) => c.city_id == routeRecord.end_city_id)
                        //                    .Return(c => c.As<City>()).ResultsAsync)
                        //                    .SingleOrDefault();

                        //await _client.Cypher.Match("(c:City)", "(r:Route)")
                        //                    .Where((City c) => c.city_id == start_city.city_id)
                        //                    .AndWhere((API.Models.Route r) => r.route_id == route.route_id)
                        //                    .CreateUnique("c-[:HAS_ROUTE]->r")
                        //                    .ExecuteWithoutResultsAsync();

                        //var startCityRoute = new CityRoute{city_id = routeRecord.start_city_id, route_id = routeRecord.route_id};
                        //var endCityRoute = new CityRoute{city_id = routeRecord.end_city_id, route_id = routeRecord.route_id};

                        //start_city.city_routes.Add(startCityRoute);
                        //end_city.city_routes.Add(endCityRoute);


                        //await _client.Cypher.Match("(c:City)")
                        //                    .Where((City c) => c.city_id == routeRecord.start_city_id)
                        //                    .Set("city_routes = $city_routes")
                        //                    .WithParam("city_routes", start_city.city_routes)
                        //                    .ExecuteWithoutResultsAsync();

                        //await _client.Cypher.Match("(c:City)")
                        //                    .Where((City c) => c.city_id == routeRecord.end_city_id)
                        //                    .Set("c.city_routes = $city_routes")
                        //                    .WithParam("city_routes", end_city.city_routes)
                        //                    .ExecuteWithoutResultsAsync();
                    }
                }

                return Ok("CSV file for routes uploaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating routes.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("delete-data", Name = "DeleteDataFromDb")]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                await _client.Cypher.OptionalMatch("(n)<-[r]-()")
                                    .Delete("r, n")
                                    .ExecuteWithoutResultsAsync();
                // Gornji valjda brise samo povezane nodeove
                await _client.Cypher.OptionalMatch("(n)")
                                    .Delete("n")
                                    .ExecuteWithoutResultsAsync();
                return Ok("All data deleted sucessfuly");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
