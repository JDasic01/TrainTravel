using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4jClient;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("find-shortest-path")]
    public class DijkstraAlgorithm : ControllerBase
    {
        private readonly IGraphClient _client;

        public DijkstraAlgorithm(IGraphClient client)
        {
            _client = client;
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

                var result = await _client.Cypher
                    .WithParams(new { startCityId, endCityId })
                    .ExecuteWithoutResultsAsync();

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

            await _client.Cypher.ExecuteCypherAsync(query);
        }
    }
}
