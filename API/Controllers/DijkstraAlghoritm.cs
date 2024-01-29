using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Neo4jClient;

namespace API.Controllers
{
    [ApiController]
    [Route("find-shortest-path")]
    public class DijkstraAlgorithm : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly Neo4jService _driver;
        private readonly ILogger<CSVFileController> _logger;
        private readonly IMemoryCache _cache;

        public DijkstraAlgorithm(IGraphClient client, ILogger<CSVFileController> logger, Neo4jService driver, IMemoryCache cache)
        {
            _client = client;
            _driver = driver;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> FindShortestPath(int startCityId, int endCityId)
        {
            var cacheKey = $"ShortestPath_{startCityId}_{endCityId}";
            if (_cache.TryGetValue(cacheKey, out string cachedResult))
            {
                _logger.LogInformation("Result retrieved from cache.");
                return Ok(cachedResult);
            }

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
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
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