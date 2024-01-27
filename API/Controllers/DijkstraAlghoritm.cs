using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Newtonsoft.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("find-shortest-path")]
    public class DijkstraAlgorithm : ControllerBase
    {
        private readonly IGraphClient _client;

        public DijkstraAlgorithm(IGraphClient client)
        {
            _client = client;
        }

    [HttpGet]
    public async Task<IActionResult> FindShortestPath(int startCityId, int endCityId)
    {
        try
        {
            var query = _client.Cypher
                .Match(
                    "p=shortestPath((start_city:City {city_id: " + startCityId + "})-[:HAS_ROUTE*]-(end_city:City {city_id: " + endCityId + "}))"
                )
                .With("p, nodes(p) AS nodes, relationships(p) AS rels")
                .Return((p, nodes, rels) => new
                {
                    Path = p.As<GraphInfo>(), // Change this to GraphInfo or a custom DTO
                    Nodes = nodes.As<List<NodeInfo>>(),
                    Relationships = rels.As<List<RelationshipInfo>>()
                });

            var queryResult = await query.ResultsAsync;
            var result = queryResult?.FirstOrDefault();

            if (result != null)
            {
                var path = result.Path;
                var nodes = result.Nodes;
                var relationships = result.Relationships;

                // Now you can access nodes and relationships as needed

                return Ok(new { Path = path, Nodes = nodes, Relationships = relationships });
            }

            return BadRequest("No path found");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error executing Dijkstra algorithm: {ex.Message}");
        }
    }


        public class PathInfo
        {
            public int StartCityId { get; set; }
            public int EndCityId { get; set; }
            // public List<string> Path { get; set; }
            // public double TotalMileage { get; set; }
        }

        public class GraphInfo
        {
            public NodeInfo Start { get; set; }
            public NodeInfo End { get; set; }
            public List<NodeInfo> Nodes { get; set; }
            public List<RelationshipInfo> Relationships { get; set; }
        }

        public class NodeInfo
        {
            public int Id { get; set; }
            public List<string> Labels { get; set; }
            public NodeProperties Properties { get; set; }
        }

        public class NodeProperties
        {
            public string CityName { get; set; }
            public int CityId { get; set; }
        }

        public class RelationshipInfo
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public int StartNodeId { get; set; }
            public int EndNodeId { get; set; }
            public RelationshipProperties Properties { get; set; }
        }

        public class RelationshipProperties
        {
            public int LineId { get; set; }
            public int Mileage { get; set; }
        }

    }
}