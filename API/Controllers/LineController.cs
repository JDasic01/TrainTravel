using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Services;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LineController : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly ICacheService _cacheService;
        private readonly IMessageService<Line> _messageService;

        public LineController(
            IGraphClient client,
            ICacheService cacheService,
            IMessageService<Line> messageService
        )
        {
            _client = client;
            _cacheService = cacheService;
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lines = await _client
                .Cypher.Match("(n:Line)")
                .Return(n => n.As<Line>())
                .ResultsAsync;

            return Ok(lines);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var line = await _client
                .Cypher.Match("(l:Line)")
                .Where((Line l) => l.line_id == id)
                .Return(l => l.As<Line>())
                .ResultsAsync;

            return Ok(line.LastOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Line line)
        {
            await _client
                .Cypher.Create("(l:Line $line)")
                .WithParam("line", line)
                .ExecuteWithoutResultsAsync();

            await _messageService.SendMessageAsync(line, Constants.lines_queue_name);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Line line)
        {
            await _client
                .Cypher.Match("(l:Line)")
                .Where((Line l) => l.line_id == id)
                .Set("l = $line")
                .WithParam("line", line)
                .ExecuteWithoutResultsAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client
                .Cypher.Match("(l:Line)")
                .Where((Line l) => l.line_id == id)
                .Delete("l")
                .ExecuteWithoutResultsAsync();
            return Ok();
        }
    }
}
