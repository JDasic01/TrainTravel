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

        public TrainRouteController(
            IGraphClient client,
            IMemoryCache cache
        )
        {
            _client = client;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var trainRoutes = await _client
                .Cypher
                .Match("(n:TrainRoute)")
                .Return(n => new
                {
                    route_id = n.As<TrainRouteCSV>().route_id,
                    line_id = n.As<TrainRouteCSV>().line_id,
                    city_ids = n.As<TrainRouteCSV>().city_ids,
                    mileage = n.As<TrainRouteCSV>().mileage
                })
                .ResultsAsync;

            var denormalizedRoutes = trainRoutes.Select(route => new
            {
                route_id = route.route_id,
                line_id = route.line_id,
                city_ids = ParseCityIds(route.city_ids),
                mileage = ParseMileage(route.mileage)
            });

            return Ok(denormalizedRoutes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var trainRoute = await _client
                .Cypher
                .Match("(n:TrainRoute {route_id: $id})")
                .WithParam("id", id)
                .Return(n => new
                {
                    route_id = n.As<TrainRouteCSV>().route_id,
                    line_id = n.As<TrainRouteCSV>().line_id,
                    city_ids = n.As<TrainRouteCSV>().city_ids,
                    mileage = n.As<TrainRouteCSV>().mileage
                })
                .ResultsAsync;

            var result = trainRoute.Select(route => new
            {
                route_id = route.route_id,
                line_id = route.line_id,
                city_ids = ParseCityIds(route.city_ids),
                mileage = ParseMileage(route.mileage)
            }).FirstOrDefault();

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TrainRouteCSV trainRoute)
        {
            await _client
                .Cypher
                .Create("(t:TrainRoute {route_id: $route_id, line_id: $line_id, city_ids: $city_ids, mileage: $mileage})")
                .WithParams(new
                {
                    trainRoute.route_id,
                    trainRoute.line_id,
                    trainRoute.city_ids,
                    trainRoute.mileage
                })
                .ExecuteWithoutResultsAsync();

            await CreateOrUpdateRoutes(trainRoute);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TrainRouteCSV trainRoute)
        {
            await _client
                .Cypher
                .Match("(t:TrainRoute)")
                .Where((TrainRouteCSV t) => t.route_id == id)
                .Delete("t")
                .ExecuteWithoutResultsAsync();

            await CreateOrUpdateRoutes(trainRoute);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client
                .Cypher
                .Match("(t:TrainRoute {route_id: $id})")
                .WithParam("id", id)
                .Delete("t")
                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        private async Task CreateOrUpdateRoutes(TrainRouteCSV trainRoute)
        {
            var trainRouteData = new TrainRoute
            {
                route_id = trainRoute.route_id,
                line_id = trainRoute.line_id,
                city_ids = ParseCityIds(trainRoute.city_ids),
                mileage = ParseMileage(trainRoute.mileage)
            };

            for (int i = 0; i < trainRouteData.city_ids.Count - 1; i++)
            {
                int currentCityId = trainRouteData.city_ids[i];
                int nextCityId = trainRouteData.city_ids[i + 1];

                await _client
                    .Cypher
                    .Match($"(c1:City {{ city_id: {currentCityId} }})")
                    .Match($"(c2:City {{ city_id: {nextCityId} }})")
                    .Merge($"(c1)-[r:HAS_ROUTE]->(c2)")
                    .Set($"r = {{ mileage: {trainRouteData.mileage[i]}, line_id: {trainRouteData.line_id} }}")
                    .ExecuteWithoutResultsAsync();

                await _client
                    .Cypher
                    .Match($"(c2:City {{ city_id: {nextCityId} }})")
                    .Match($"(c1:City {{ city_id: {currentCityId} }})")
                    .Merge($"(c2)-[r:HAS_ROUTE]->(c1)")
                    .Set($"r = {{ mileage: {trainRouteData.mileage[i]}, line_id: {trainRouteData.line_id} }}")
                    .ExecuteWithoutResultsAsync();
            }

            var firstCity = trainRouteData.city_ids.First();
            var lastCity = trainRouteData.city_ids.Last();
            _cache.Remove($"ShortestPath_{firstCity}_{lastCity}");
        }

        private List<int> ParseCityIds(string cityIds) =>
            cityIds.Trim('[', ']').Split(',').Select(id => int.Parse(id.Trim())).ToList();

        private List<int> ParseMileage(string mileage) =>
            mileage.Trim('[', ']').Split(',').Select(m => int.Parse(m.Trim())).ToList();
    }
}
