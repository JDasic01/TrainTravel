using Microsoft.AspNetCore.Mvc;
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

        public CSVFileController(IGraphClient client)
        {
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
                            city_routes = new HashSet<CityRoute>(),
                            city_to_city = new HashSet<CityToCity>()
                        };
                        //await _client.Cypher.Create("(c:City $city)")
                        //                    .WithParam("city", city)
                        //                    .ExecuteWithoutResultsAsync();

                        // Doda ako ne postoji grad s tim Id-em
                        await _client.Cypher.Merge("(c:City { city_id: {id} })")
                                                .OnCreate()
                                                .Set("c = {city}")
                                                .WithParams(new {city_id = city.city_id, city})
                                                .ExecuteWithoutResultsAsync();
                    }  
                }

                return Ok("CSV file for cities uploaded successfully.");
            }
            catch (Exception ex)
            {
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
                            end_city_id = routeRecord.end_city_id
                        };
                        
                        await _client.Cypher.Create("(r:Route $route)")
                                            .WithParam("route", route)
                                            .ExecuteWithoutResultsAsync();
                        
                        // start end city relation
                        var start_city = (await _client.Cypher.Match("(c:City)")
                                                .Where((City c) => c.city_id == routeRecord.start_city_id)
                                                .Return(c => c.As<City>()).ResultsAsync)
                                                .SingleOrDefault();

                        var end_city = (await _client.Cypher.Match("(c:City)")
                                            .Where((City c) => c.city_id == routeRecord.end_city_id)
                                            .Return(c => c.As<City>()).ResultsAsync)
                                            .SingleOrDefault();
                        var startCityRoute = new CityRoute{city_id = routeRecord.start_city_id, route_id = routeRecord.route_id};
                        var endCityRoute = new CityRoute{city_id = routeRecord.end_city_id, route_id = routeRecord.route_id};
                        start_city.city_routes.Add(startCityRoute);
                        end_city.city_routes.Add(endCityRoute);


                        await _client.Cypher.Match("(c:City)")
                                            .Where((City c) => c.city_id == routeRecord.start_city_id)
                                            .Set("c = $city")
                                            .WithParam("city", start_city)
                                            .ExecuteWithoutResultsAsync();

                        await _client.Cypher.Match("(c:City)")
                                            .Where((City c) => c.city_id == routeRecord.end_city_id)
                                            .Set("c = $city")
                                            .WithParam("city", end_city)
                                            .ExecuteWithoutResultsAsync();
                    }
                }

                return Ok("CSV file for routes uploaded successfully.");
            }
            catch (Exception ex)
            {
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
                await _client.Cypher.Match("(n)")
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
