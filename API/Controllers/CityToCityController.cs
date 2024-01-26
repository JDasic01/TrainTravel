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
    public class CityToCityController : ControllerBase
    {
        
        private readonly IGraphClient _client;

        public CityToCityController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get(){
             var Connections = await _client.Cypher.Match("(:City)-[n:C_TO_C]->(:City)")
                                                   .Return(n => n.As<CityToCity>()).ResultsAsync;

            return Ok(Connections);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int city_id_1, int city_id_2){
                var Connections = await _client.Cypher.Match("((:City)-[r:C_TO_C]->(:City)")
                                                .Where((City c1) => c1.city_id == city_id_1)
                                                .AndWhere((City c2) => c2.city_id == city_id_2)
                                                .Return(r => r.As<CityToCity>()).ResultsAsync;

                return Ok(Connections.LastOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CityToCity cityToCity){
            await _client.Cypher.Match("(c1:City), (c2:City)")
                                .Where((City c1) => c1.city_id == cityToCity.city_id_1)
                                .AndWhere((City c2) => c2.city_id == cityToCity.city_id_2)
                                .Create("(c1)-[r:C_TO_C]->(c2) $relationship")
                                .WithParam("relationship", cityToCity)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]CityToCity cityToCity){
                await _client.Cypher.Match("(:City)-[r:C_TO_C]->(:City)")
                                    .Where((City c1) => c1.city_id == cityToCity.city_id_1)
                                    .AndWhere((City c2) => c2.city_id == cityToCity.city_id_2)
                                    .Set("r = $CityToCity")
                                    .WithParam("CityToCity", cityToCity)
                                    .ExecuteWithoutResultsAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int city_id_1, int city_id_2){
                await  _client.Cypher.Match("((:City)-[r:C_TO_C]->(:City)")
                                    .Where((City c1) => c1.city_id == city_id_1)
                                    .AndWhere((City c2) => c2.city_id == city_id_2)
                                    .Delete("r")
                                    .ExecuteWithoutResultsAsync();
                return Ok();
        }
    }
}