using Microsoft.Extensions.Caching.Memory;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrainRouteController : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly IMemoryCache _cache;
        private readonly IMessageService<Message> _messageService;

        public TrainRouteController(
            IGraphClient client,
            IMemoryCache cache,
            IMessageService<Message> messageService
        )
        {
            _client = client;
            _cache = cache;
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var trainRoutes = await _client
                .Cypher.Match("(n:TrainRoute)")
                .OptionalMatch("(n)-[:HAS_LINE]->(city:City)")
                .Return((n, city) => new
                {
                    route_id = n.As<TrainRouteCSV>().route_id,
                    line_id = n.As<TrainRouteCSV>().line_id,
                    city_ids = n.As<TrainRouteCSV>().city_ids,
                    mileage = n.As<TrainRouteCSV>().mileage,
                })
                .ResultsAsync;

            var denormalizedRoutes = trainRoutes.Select(route => new
            {
                route_id = route.route_id,
                line_id = route.line_id,
                city_ids = ParseCityIds(route.city_ids),
                mileage = ParseMileage(route.mileage),
            });

            return Ok(denormalizedRoutes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var trainRoute = await _client
                .Cypher.Match("(n:TrainRoute)")
                .OptionalMatch("(n)-[:HAS_LINE]->(city:City)")
                .Where((TrainRoute n) => n.route_id == id)
                .Return((n, city) => new
                {
                    route_id = n.As<TrainRouteCSV>().route_id,
                    line_id = n.As<TrainRouteCSV>().line_id,
                    city_ids = n.As<TrainRouteCSV>().city_ids,
                    mileage = n.As<TrainRouteCSV>().mileage,
                })
                .ResultsAsync;

            var denormalizedRoutes = trainRoute.Select(route => new
            {
                route_id = route.route_id,
                line_id = route.line_id,
                city_ids = ParseCityIds(route.city_ids),
                mileage = ParseMileage(route.mileage),
            });

            var result = denormalizedRoutes.FirstOrDefault();

            if (result == null)
            {
                return NotFound(); 
            }

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TrainRouteCSV trainRoute)
        {
            await _client
                    .Cypher.Create(
                           "(t:TrainRoute {route_id: $route_id, line_id: $line_id, city_ids: $city_ids, mileage: $mileage})"
                            )
                            .WithParam("route_id", trainRoute.route_id)
                            .WithParam("line_id", trainRoute.line_id)
                            .WithParam("city_ids", trainRoute.city_ids)
                            .WithParam("mileage", trainRoute.mileage)
                            .ExecuteWithoutResultsAsync();

            TrainRoute trainRoute1 = new TrainRoute();
            trainRoute1.route_id = trainRoute.route_id;
            trainRoute1.line_id = trainRoute.line_id;
            trainRoute1.city_ids = ParseCityIds(trainRoute.city_ids);
            trainRoute1.mileage = ParseMileage(trainRoute.mileage);
            for (int i = 0; i < trainRoute1.city_ids.Count - 1; i++)
            {
                int currentCityId = trainRoute1.city_ids[i];
                int nextCityId = trainRoute1.city_ids[i + 1];

                await _client
                    .Cypher.Match(
                        $"(c{currentCityId}:City {{ city_id: {currentCityId} }})"
                    )
                    .Match($"(c{nextCityId}:City {{ city_id: {nextCityId} }})")
                    .Merge(
                        $"(c{currentCityId})-[r{currentCityId}{nextCityId}:HAS_ROUTE]->(c{nextCityId})"
                    )
                    .Set(
                        $"r{currentCityId}{nextCityId} = {{ mileage: {trainRoute1.mileage[i]}, line_id: {trainRoute1.line_id} }}"
                    )
                    .ExecuteWithoutResultsAsync();

                await _client
                    .Cypher.Match(
                        $"(c{nextCityId}:City {{ city_id: {nextCityId} }})"
                    )
                    .Match($"(c{currentCityId}:City {{ city_id: {currentCityId} }})")
                    .Merge(
                        $"(c{nextCityId})-[r{nextCityId}{currentCityId}:HAS_ROUTE]->(c{currentCityId})"
                    )
                    .Set(
                        $"r{nextCityId}{currentCityId} = {{ mileage: {trainRoute1.mileage[i]}, line_id: {trainRoute1.line_id} }}"
                    )
                    .ExecuteWithoutResultsAsync();
            }

            var first = trainRoute1.city_ids.First();
            var last = trainRoute1.city_ids.Last();
            var cacheKey = $"ShortestPath_{first}_{last}";
            _cache.Remove(cacheKey);
            await _messageService.SendMessageAsync(new Message(first, last), "line_queue");

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TrainRouteCSV trainRoute)
        {
            try
            {
                await _client
                    .Cypher.Match("(t:TrainRoute)")
                    .Where((TrainRoute t) => t.route_id == id)
                    .DetachDelete("t")
                    .ExecuteWithoutResultsAsync();

                await _client
                    .Cypher.Create(
                        "(t:TrainRoute {route_id: $route_id,line_id: $line_id, city_ids: $city_ids, mileage: $mileage})"
                    )
                    .WithParam("route_id", trainRoute.route_id)
                    .WithParam("line_id", trainRoute.line_id)
                    .WithParam("city_ids", trainRoute.city_ids)
                    .WithParam("mileage", trainRoute.mileage)
                    .ExecuteWithoutResultsAsync();

                TrainRoute trainRoute1 = new TrainRoute();
                trainRoute1.route_id = trainRoute.route_id;
                trainRoute1.line_id = trainRoute.line_id;
                trainRoute1.city_ids = ParseCityIds(trainRoute.city_ids);
                trainRoute1.mileage = ParseMileage(trainRoute.mileage);

                for (int i = 0; i < trainRoute1.city_ids.Count - 1; i++)
                {
                    int currentCityId = trainRoute1.city_ids[i];
                    int nextCityId = trainRoute1.city_ids[i + 1];

                    await _client
                        .Cypher.Match(
                            $"(c{currentCityId}:City {{ city_id: {currentCityId} }})"
                        )
                        .Match($"(c{nextCityId}:City {{ city_id: {nextCityId} }})")
                        .Merge(
                            $"(c{currentCityId})-[r{currentCityId}{nextCityId}:HAS_ROUTE]->(c{nextCityId})"
                        )
                        .Set(
                            $"r{currentCityId}{nextCityId} = {{ mileage: {trainRoute1.mileage[i]}, line_id: {trainRoute1.line_id} }}"
                        )
                        .ExecuteWithoutResultsAsync();

                    await _client
                        .Cypher.Match(
                            $"(c{nextCityId}:City {{ city_id: {nextCityId} }})"
                        )
                        .Match($"(c{currentCityId}:City {{ city_id: {currentCityId} }})")
                        .Merge(
                            $"(c{nextCityId})-[r{nextCityId}{currentCityId}:HAS_ROUTE]->(c{currentCityId})"
                        )
                        .Set(
                            $"r{nextCityId}{currentCityId} = {{ mileage: {trainRoute1.mileage[i]}, line_id: {trainRoute1.line_id} }}"
                        )
                        .ExecuteWithoutResultsAsync();
                }

                var first = trainRoute1.city_ids.First();
                var last = trainRoute1.city_ids.Last();
                var cacheKey = $"ShortestPath_{first}_{last}";
                _cache.Remove(cacheKey);
                await _messageService.SendMessageAsync(new Message(first, last), "line_queue");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client
                .Cypher.Match("(t:TrainRoute)")
                .Where((TrainRoute t) => t.route_id == id)
                .Delete("t")
                .ExecuteWithoutResultsAsync();
            return Ok();
        }

        private List<int> ParseCityIds(string cityIds)
        {
            return cityIds.Trim('[', ']').Split(',').Select(id => int.Parse(id.Trim())).ToList();
        }

        private List<int> ParseMileage(string mileage)
        {
            return mileage.Trim('[', ']').Split(',').Select(m => int.Parse(m.Trim())).ToList();
        }

        private string SerializeCityIds(List<int> cityIds)
        {
            return "[" + string.Join(",", cityIds) + "]";
        }

        private string SerializeMileage(List<int> mileage)
        {
            return "[" + string.Join(",", mileage) + "]";
        }

    }
}