using API.Models; 
using Neo4jClient;

public class Neo4jService
{
    private readonly IGraphClient _graphClient;

    public Neo4jService(string neo4jUri, string neo4jUsername, string neo4jPassword)
    {
        _graphClient = new GraphClient(new Uri(neo4jUri), neo4jUsername, neo4jPassword);
        _graphClient.Connect();
    }

    public void CreateCity(City city)
    {
        _graphClient.Cypher
            .Create("(c:City {newCity})")
            .WithParam("newCity", city)
            .ExecuteWithoutResults();
    }

    public IEnumerable<City> GetCities()
    {
        return _graphClient.Cypher
            .Match("(c:City)")
            .Return<City>("c")
            .Results;
    }
}
