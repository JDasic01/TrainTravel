using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CityController : ControllerBase
    {
        private readonly ILogger<CityController> _logger;
        private readonly IGraphClient _client;

        public CityController(
            IGraphClient client,
            ILogger<CityController> logger
        )
        {
            _logger = logger;
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cities = await _client
                .Cypher.Match("(n:City)")
                .Return(n => n.As<City>())
                .ResultsAsync;

            return Ok(cities);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _client
                    .Cypher.Match("(c:City)")
                    .OptionalMatch("(c)-[r:HAS_LINE]->(other:City)")
                    .Return(
                        (c, r, other) =>
                            new
                            {
                                City = c.As<City>(),
                                Lines = r.CollectAs<Line>(),
                                OtherCities = other.CollectAs<City>()
                            }
                    )
                    .ResultsAsync;

                var cityInfo = result
                    .Where(item => item.City.city_id == id)
                    .Select(item => new
                    {
                        CityId = item.City.city_id,
                        CityName = item.City.city_name,
                        AvailableRoutes = item
                            .Lines.Select(line => new
                            {
                                LineId = line.line_id,
                                EndCityId = item.OtherCities.First().city_id,
                                EndCityName = item.OtherCities.First().city_name
                            })
                            .ToList()
                    })
                    .FirstOrDefault();

                if (cityInfo == null)
                {
                    return NotFound($"City with ID {id} not found.");
                }

                return Ok(cityInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving city with lines.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] City city)
        {
            await _client
                .Cypher.Create("(c:City $city)")
                .WithParam("city", city)
                .ExecuteWithoutResultsAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] City city)
        {
            await _client
                .Cypher.Match("(c:City)")
                .Where((City c) => c.city_id == id)
                .Set("c = $city")
                .WithParam("city", city)
                .ExecuteWithoutResultsAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client
                .Cypher
                .Match("(start:City)-[r:HAS_ROUTE]-(end:City)")
                .Where((City start) => start.city_id == id)
                .OrWhere((City end) => end.city_id == id)
                .Delete("r")
                .ExecuteWithoutResultsAsync();

            await _client
                .Cypher
                .Match("(start:City)-[r:HAS_LINE]-(end:City)")
                .Where((City start) => start.city_id == id)
                .OrWhere((City end) => end.city_id == id)
                .Delete("r")
                .ExecuteWithoutResultsAsync();

            await _client
                .Cypher.Match("(c:City)")
                .Where((City c) => c.city_id == id)
                .Delete("c")
                .ExecuteWithoutResultsAsync();
            return Ok();
        }
    }
}
