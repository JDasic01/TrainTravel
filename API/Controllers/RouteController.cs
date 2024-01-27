using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly ICacheService _cacheService;
        private readonly IMessageService<API.Models.Route> _messageService;

        public RouteController(
            IGraphClient client,
            ICacheService cacheService,
            IMessageService<API.Models.Route> messageService
        )
        {
            _client = client;
            _cacheService = cacheService;
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var routes = await _client
                .Cypher.Match("(n:Route)")
                .Return(n => n.As<API.Models.Route>())
                .ResultsAsync;

            return Ok(routes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var route = await _client
                .Cypher.Match("(r:Route)")
                .Where((API.Models.Route r) => r.route_id == id)
                .Return(r => r.As<API.Models.Route>())
                .ResultsAsync;

            return Ok(route.LastOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] API.Models.Route route)
        {
            await _client
                .Cypher.Create("(r:Route $route)")
                .WithParam("route", route)
                .ExecuteWithoutResultsAsync();

            await _messageService.SendMessageAsync(route, Constants.routes_queue_name);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] API.Models.Route route)
        {
            await _client
                .Cypher.Match("(r:Route)")
                .Where((API.Models.Route r) => r.route_id == id)
                .Set("r = $route")
                .WithParam("route", route)
                .ExecuteWithoutResultsAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client
                .Cypher.Match("(r:Route)")
                .Where((API.Models.Route r) => r.route_id == id)
                .Delete("r")
                .ExecuteWithoutResultsAsync();
            return Ok();
        }
    }
}
