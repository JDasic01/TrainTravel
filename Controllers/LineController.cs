using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LineController : ControllerBase
    {
        private readonly IGraphClient _client;

        public LineController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var relations = await _client
                    .Cypher
                    .Match("(c1:City)-[r:HAS_LINE]->(c2:City)")
                    .Return((c1, r, c2) => new
                    {
                        line_id = r.As<Line>().line_id,
                        line_name = r.As<Line>().line_name,
                        start_city_id = c1.As<City>().city_id,
                        end_city_id = c2.As<City>().city_id
                    })
                    .ResultsAsync;

                if (relations == null || !relations.Any())
                {
                    return Ok(new List<Line>()); 
                }

                return Ok(relations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving lines: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var lineInfo = await _client
                    .Cypher
                    .Match("(c1:City)-[r:HAS_LINE]->(c2:City)")
                    .Where((Line r) => r.line_id == id)
                    .Return((c1, r, c2) => new
                    {
                        line_id = r.As<Line>().line_id,
                        line_name = r.As<Line>().line_name,
                        start_city_id = c1.As<City>().city_id,
                        end_city_id = c2.As<City>().city_id
                    })
                    .ResultsAsync;

                var line = lineInfo.FirstOrDefault();

                if (line == null)
                {
                    return NotFound($"Line with ID {id} not found.");
                }

                return Ok(line);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving line by ID: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Line line)
        {
            try
            {
                var startCityExists = await _client
                    .Cypher.Match("(c1:City { city_id: $startCityId })")
                    .WithParam("startCityId", line.start_city_id)
                    .Return(c1 => c1.As<City>())
                    .ResultsAsync;

                var endCityExists = await _client
                    .Cypher.Match("(c2:City { city_id: $endCityId })")
                    .WithParam("endCityId", line.end_city_id)
                    .Return(c2 => c2.As<City>())
                    .ResultsAsync;

                if (!startCityExists.Any() || !endCityExists.Any())
                {
                    return NotFound("One or both cities referenced in the line do not exist.");
                }

                await _client
                    .Cypher.Match("(c1:City { city_id: $startCityId })")
                    .Match("(c2:City { city_id: $endCityId })")
                    .Merge($"(c1)-[r1:HAS_LINE]->(c2)")
                    .Set("r1 = $line")
                    .WithParam("startCityId", line.start_city_id)
                    .WithParam("endCityId", line.end_city_id)
                    .WithParam("line", new { line.line_id, line.line_name })
                    .ExecuteWithoutResultsAsync();

                string[] parts = line.line_name.Split('-');
                var oppositeName = $"{parts[1]}-{parts[0]}";

                await _client
                    .Cypher.Match("(c2:City { city_id: $endCityId })")
                    .Match("(c1:City { city_id: $startCityId })")
                    .Merge($"(c2)-[r2:HAS_LINE]->(c1)")
                    .Set("r2 = $line")
                    .WithParam("startCityId", line.start_city_id)
                    .WithParam("endCityId", line.end_city_id)
                    .WithParam("line", new { line.line_id, line_name = oppositeName })
                    .ExecuteWithoutResultsAsync();

                return Ok("Line created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating line: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Line line)
        {
            try
            {
                var lineExists = await _client
                    .Cypher
                    .Match("(c1:City)-[r:HAS_LINE]->(c2:City)")
                    .Where((Line r) => r.line_id == id)
                    .Return(r => r.As<Line>())
                    .ResultsAsync;

                if (!lineExists.Any())
                {
                    return NotFound($"Line with ID {id} does not exist.");
                }

                string[] parts = line.line_name.Split('-');
                var oppositeName = $"{parts[1]}-{parts[0]}";

                await _client
                    .Cypher.Match("(c1:City { city_id: $startCityId })")
                    .Match("(c2:City { city_id: $endCityId })")
                    .Merge($"(c1)-[r1:HAS_LINE]->(c2)")
                    .Set("r1 = $line")
                    .WithParam("startCityId", line.start_city_id)
                    .WithParam("endCityId", line.end_city_id)
                    .WithParam("line", new { line.line_id, line.line_name })
                    .ExecuteWithoutResultsAsync();

                await _client
                    .Cypher.Match("(c2:City { city_id: $endCityId })")
                    .Match("(c1:City { city_id: $startCityId })")
                    .Merge($"(c2)-[r2:HAS_LINE]->(c1)")
                    .Set("r2 = $line")
                    .WithParam("startCityId", line.start_city_id)
                    .WithParam("endCityId", line.end_city_id)
                    .WithParam("line", new { line.line_id, line_name = oppositeName })
                    .ExecuteWithoutResultsAsync();

                return Ok("Line updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating line: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var lineExists = await _client
                    .Cypher
                    .Match("(c1:City)-[r:HAS_LINE]->(c2:City)")
                    .Where((Line r) => r.line_id == id)
                    .Return(r => r.As<Line>())
                    .ResultsAsync;

                if (!lineExists.Any())
                {
                    return NotFound($"Line with ID {id} does not exist.");
                }

                await _client
                    .Cypher
                    .Match("(c1:City)-[r1:HAS_LINE]->(c2:City)")
                    .Where((Line r1) => r1.line_id == id)
                    .Delete("r1")
                    .ExecuteWithoutResultsAsync();

                await _client
                    .Cypher
                    .Match("(c2:City)-[r2:HAS_LINE]->(c1:City)")
                    .Where((Line r2) => r2.line_id == id)
                    .Delete("r2")
                    .ExecuteWithoutResultsAsync();

                return Ok("Line deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting line: {ex.Message}");
            }
        }
    }
}
