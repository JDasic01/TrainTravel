namespace API.Services
{
    public interface ICacheService
    {
        string city_db_name { get; }
        string route_db_name { get; }

        T GetData<T>(string key);
        bool SetData<T>(string key, T value, DateTimeOffset expirationTime);
        bool RemoveData(string key);
        Task<T> GetRequest<T>(string key);
        void PostRequest<T>(string key, int id, T value);
        void DeleteRequest(string key, int id);
    }
}
