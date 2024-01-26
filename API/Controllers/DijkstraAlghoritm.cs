using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4jClient;
using Microsoft.AspNetCore.Mvc;
using API.Services;

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
        public async Task<IActionResult> FindShortestPath(string startCityId, string endCityId)
        {
            await CreateGraphProjection();
            
            try
            {
                var query = @"
                    CALL gds.graph.project(
                        'yourGraphName',
                        ['YourNodeLabel', 'AnotherLabel'],
                        ['HAS_ROUTE'],
                        {
                            relationshipProperties: 'yourCostProperty'
                        }
                    )
                ";

                return Ok("Graph projection executed successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error executing graph projection: {ex.Message}");
            }
        }

        private async Task CreateGraphProjection()
        {
            // Replace 'yourGraphName', 'YourNodeLabel', 'YOUR_RELATIONSHIP_TYPE', and 'yourCostProperty'
            var graphName = "ShortestRoute";
            var nodeLabel = "YourNodeLabel";
            var relationshipType = "C_TO_C";
            var relationshipProperty = "yourCostProperty";

            // Cypher query to create graph projection
            var query = $@"
                CALL gds.graph.create(
                    '{graphName}', 
                    {{
                        {nodeLabel}: {{
                            label: '{nodeLabel}'
                        }}
                    }}, 
                    {{
                        {relationshipType}: {{
                            type: '{relationshipType}',
                            properties: '{relationshipProperty}'
                        }}
                    }}
                );
            ";
        }
    }
}
