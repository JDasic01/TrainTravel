using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CityController : ControllerBase
    {
        
        private readonly IGraphClient _client;

        public CityController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get(){
             var Cities = await _client.Cypher.Match("(n: City)")
                                                   .Return(n => n.As<City>()).ResultsAsync;

            return Ok(Cities);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id){
                var Cities = await _client.Cypher.Match("(c:City)")
                                                    .Where((City c) => c.city_id == id)
                                                    .Return(c => c.As<City>()).ResultsAsync;

                return Ok(Cities.LastOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]City city){
            await _client.Cypher.Create("(c:City $city)")
                                .WithParam("city", city)
                                .ExecuteWithoutResultsAsync();

                return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]City city){
                await _client.Cypher.Match("(c:City)")
                                    .Where((City c) => c.city_id == id)
                                    .Set("c = $city")
                                    .WithParam("city", city)
                                    .ExecuteWithoutResultsAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id){
                await  _client.Cypher.Match("(c:City)")
                                    .Where((City c) => c.city_id == id)
                                    .Delete("c")
                                    .ExecuteWithoutResultsAsync();
                return Ok();
        }
    }
}