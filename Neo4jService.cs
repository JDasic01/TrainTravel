using Neo4j.Driver;

public class Neo4jService : IDisposable
{
    private readonly IDriver _driver;

    public Neo4jService(string uri, string user, string password)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    public IAsyncSession GetSession(string dbname)
    {
        return _driver.AsyncSession(o => o.WithDatabase(dbname));
    }

    public void Dispose()
    {
        _driver?.Dispose();
    }
}
