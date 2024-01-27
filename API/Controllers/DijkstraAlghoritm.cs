using System;
using System.Threading.Tasks;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Npgsql;


namespace API.Controllers
{
    [ApiController]
    [Route("find-shortest-path")]
    public class DijkstraAlgorithm : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly ICacheService _cacheService;

        public DijkstraAlgorithm(IGraphClient client, ICacheService cacheService)
        {
            _client = client;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> FindShortestPath(int startCityId, int endCityId)
        {
            try
            {
                var minMileage = 1000;

                var query = _client
                    .Cypher.Match(
                        "p=shortestPath((start_city:City {city_id: "
                            + startCityId
                            + "})-[*]-(end_city:City {city_id: "
                            + endCityId
                            + "}))"
                    )
                    .Where("ALL(rel IN relationships(p) WHERE rel.mileage <= " + minMileage + ")")
                    .WithParam("startCityId", startCityId)
                    .WithParam("endCityId", endCityId)
                    .WithParam("minMileage", minMileage)
                    .Return<object>("p");

                // Deserialize the result into a dynamic object (plaky)
                var queryResult = await query.ResultsAsync; // Await the asynchronous call
                var result = queryResult?.FirstOrDefault();
                Console.WriteLine(result);
                if (result != null)
                {
                    var resultJson = JsonConvert.SerializeObject(result);
                    Console.WriteLine(resultJson);
                    var deserializedResult = JsonConvert.DeserializeObject<dynamic>(resultJson);

                    // Extract information from the deserialized result
                    var startCity = deserializedResult?.start?.properties?.city_name?.ToString();
                    var endCity = deserializedResult?.end?.properties?.city_name?.ToString();
                    var segments = deserializedResult?.segments as JArray;
                    var path = segments
                        ?.Select(s => s["end"]["properties"]["city_name"].ToString())
                        .ToList();
                    var totalMileage = segments?.Sum(s =>
                        (double)s["relationship"]["properties"]["mileage"]
                    );

                    // Create a custom DTO
                    var pathInfo = new PathInfo
                    {
                        StartCity = startCity,
                        EndCity = endCity,
                        Path = path,
                        TotalMileage = totalMileage ?? 0.0,
                    };

                    // Return the structured response
                    return Ok(deserializedResult);
                }

                return BadRequest("bed");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error executing Dijkstra algorithm: {ex.Message}");
            }
        }

        public class PathInfo
        {
            public string StartCity { get; set; }
            public string EndCity { get; set; }
            public List<string> Path { get; set; }
            public double TotalMileage { get; set; }
        }
    }
}
