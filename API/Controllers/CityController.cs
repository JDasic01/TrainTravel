using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using API.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CityController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IGraphClient _client;
        private readonly ICacheService _cacheService;

        public CityController(IGraphClient client, IMessageService messageService, ICacheService cacheService)
        {
            _client = client;
            _messageService = messageService;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cachedCities = await _cacheService.GetRequest<IEnumerable<City>>(_cacheService.city_db_name);

            if (cachedCities != null && cachedCities.Any())
            {
                return Ok(cachedCities);
            }

            var cities = await _client.Cypher.Match("(n:City)").Return(n => n.As<City>()).ResultsAsync;

            _cacheService.SetData(_cacheService.city_db_name, cities, DateTimeOffset.Now.AddSeconds(30));

            return Ok(cities);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cachedCity = await _cacheService.GetRequest<City>(_cacheService.city_db_name + id);

            if (cachedCity != null)
            {
                return Ok(cachedCity);
            }

            var Cities = await _client.Cypher.Match("(c:City)")
                                                    .Where((City c) => c.city_id == id)
                                                    .Return(c => c.As<City>()).ResultsAsync;

            var city = Cities.LastOrDefault();
            if (city != null)
            {
                _cacheService.SetData(_cacheService.city_db_name + id, city, DateTimeOffset.Now.AddSeconds(30));
                return Ok(city);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]City city)
        {
            await _client.Cypher.Create("(c:City $city)")
                                .WithParam("city", city)
                                .ExecuteWithoutResultsAsync();

            _cacheService.PostRequest(_cacheService.city_db_name, city.city_id, city);
            _messageService.SendMessageAsync(city);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]City city)
        {
            await _client.Cypher.Match("(c:City)")
                                .Where((City c) => c.city_id == id)
                                .Set("c = $city")
                                .WithParam("city", city)
                                .ExecuteWithoutResultsAsync();

            _cacheService.PostRequest(_cacheService.city_db_name, id, city);
            _messageService.SendMessageAsync(city);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client.Cypher.Match("(c:City)")
                                .Where((City c) => c.city_id == id)
                                .Delete("c")
                                .ExecuteWithoutResultsAsync();

            _cacheService.DeleteRequest(_cacheService.city_db_name, id);

            return Ok();
        }
    }
}
