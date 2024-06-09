using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System.Linq;
using System.Threading.Tasks;

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
            try
            {
                var cities = await _client
                    .Cypher.Match("(c:City)")
                    .Return(c => new
                    {
                        c.As<City>().id,
                        c.As<City>().name,
                        c.As<City>().seeSection,
                        c.As<City>().doSection,
                        c.As<City>().guide,
                        c.As<City>().availableRoutes
                    }).ResultsAsync;

                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cities.");
                return StatusCode(500, "Internal server error");
            }
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
                    .Where(item => item.City.id == id)
                    .Select(item => new
                    {
                        CityId = item.City.id,
                        CityName = item.City.name,
                        AvailableRoutes = item
                            .Lines.Select(line => new
                            {
                                LineId = line.id,
                                EndCityId = item.OtherCities.FirstOrDefault()?.id,
                                EndCityName = item.OtherCities.FirstOrDefault()?.name
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
            try
            {
                await _client
                    .Cypher.Create("(c:City $city)")
                    .WithParam("city", city)
                    .ExecuteWithoutResultsAsync();
                
                return Ok(city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating city.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] City city)
        {
            try
            {
                await _client
                    .Cypher.Match("(c:City)")
                    .Where((City c) => c.id == id)
                    .Set("c = $city")
                    .WithParam("city", city)
                    .ExecuteWithoutResultsAsync();
                
                return Ok(city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating city.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Delete relationships first
                await _client
                    .Cypher
                    .Match("(start:City)-[r:HAS_ROUTE|HAS_LINE]-(end:City)")
                    .Where((City start) => start.id == id)
                    .OrWhere((City end) => end.id == id)
                    .Delete("r")
                    .ExecuteWithoutResultsAsync();

                // Delete the city node
                await _client
                    .Cypher.Match("(c:City)")
                    .Where((City c) => c.id == id)
                    .Delete("c")
                    .ExecuteWithoutResultsAsync();
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting city.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
