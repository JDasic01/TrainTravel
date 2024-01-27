using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using API.Services;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neo4jClient;

namespace API.Controllers
{
    [ApiController]
    [Route("csv-file-upload")]
    public class CSVFileController : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly ILogger<CSVFileController> _logger;
        private readonly ICacheService _cacheService;

        public CSVFileController(
            ILogger<CSVFileController> logger,
            IGraphClient client,
            ICacheService cacheService
        )
        {
            _logger = logger;
            _client = client;
            _cacheService = cacheService;
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
                        };

                        await _client
                            .Cypher.Create("(c:City $city)")
                            .WithParam("city", city)
                            .ExecuteWithoutResultsAsync();

                        _cacheService.PostRequest(_cacheService.city_db_name, city.city_id, city);
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
                        };

                        await _client
                            .Cypher.Match("(c1:City { city_id: " + route.start_city_id + " })")
                            .Match("(c2:City { city_id: " + route.end_city_id + " })")
                            .Merge($"(c1)-[r1:HAS_LINE]->(c2)")
                            .Set($"r1 = {{ mileage: {route.mileage}, route_id: {route.route_id} }}")
                            .ExecuteWithoutResultsAsync();

                        await _client
                            .Cypher.Match("(c2:City { city_id: " + route.end_city_id + " })")
                            .Match("(c1:City { city_id: " + route.start_city_id + " })")
                            .Merge($"(c2)-[r2:HAS_LINE]->(c1)")
                            .Set($"r2 = {{ mileage: {route.mileage}, route_id: {route.route_id} }}")
                            .ExecuteWithoutResultsAsync();

                        _cacheService.PostRequest(
                            _cacheService.route_db_name,
                            route.route_id,
                            route
                        );
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
                await _client
                    .Cypher.OptionalMatch("(n)<-[r]-()")
                    .Delete("r, n")
                    .ExecuteWithoutResultsAsync();
                // Gornji valjda brise samo povezane nodeove
                await _client.Cypher.OptionalMatch("(n)").Delete("n").ExecuteWithoutResultsAsync();
                return Ok("All data deleted sucessfuly");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}