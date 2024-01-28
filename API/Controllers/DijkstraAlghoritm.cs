using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("find-shortest-path")]
    public class DijkstraAlgorithm : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly Neo4jService _driver;
        private readonly ILogger<CSVFileController> _logger;

        public DijkstraAlgorithm(IGraphClient client, ILogger<CSVFileController> logger, Neo4jService driver)
        {
            _client = client;
            _driver = driver;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> FindShortestPath(int startCityId, int endCityId)
        {
            var session = _driver.GetSession("neo4j");
            try
            {
                var query = _client.Cypher
                .Match(
                    "p=shortestPath((start_city:City {city_id: " + startCityId + "})-[:HAS_ROUTE*]-(end_city:City {city_id: " + endCityId + "}))"
                )
                .Return<string>("p");
                var queryResult = await query.ResultsAsync;
                var result = queryResult?.FirstOrDefault();

                _logger.LogInformation(result);

                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest("No path found");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error executing Dijkstra algorithm: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }
        }
    }
}