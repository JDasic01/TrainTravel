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
                            city_routes = null, 
                            city_to_city = null 
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


                        await _client.Cypher.Match("(c:City), (r:Route)")
                                            .Where((City c) => c.city_id == routeRecord.start_city_id)
                                            .AndWhere((API.Models.Route r) => r.route_id == routeRecord.route_id)
                                            .Create("(c1)-[:HAS_ROUTE]->(r)<-[:HAS_ROUTE]-(c2)")
                                            .ExecuteWithoutResultsAsync();

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

        [HttpPost("upload-city-connections", Name = "UploadCityToCityCSV")]
        public async Task<IActionResult> CreateCityConnections(IFormFile formFile)
        {
            try
            {
                using (var reader = new StreamReader(formFile.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var connectionRecords = csv.GetRecords<CityToCityCSVModel>();
                    foreach (var connectionRecord in connectionRecords)
                    {
                        var relationship = new CityToCity
                        {
                            city_id_1 = connectionRecord.city_id_1,
                            city_id_2 = connectionRecord.city_id_2,
                            mileage = connectionRecord.mileage
                        };

                        await _client.Cypher.Match("(c1:City), (c2:City)")
                                            .Where((City c1) => c1.city_id == connectionRecord.city_id_1)
                                            .AndWhere((City c2) => c2.city_id == connectionRecord.city_id_2)
                                            .Create("(c1)-[r:C_TO_C]->(c2) $relationship")
                                            .WithParam("relationship", relationship)
                                            .ExecuteWithoutResultsAsync();
                    }
                }

                return Ok("CSV file for city connection uploaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating city connections.");
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
