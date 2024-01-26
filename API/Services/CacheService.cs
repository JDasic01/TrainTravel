using StackExchange.Redis;
using System;
using System.Text.Json;

namespace API.Services
{
    public class CacheService : ICacheService
    {
        private IDatabase _cacheDb;

        public CacheService()
        {
            var redis = ConnectionMultiplexer.Connect("redis:6379");
            _cacheDb = redis.GetDatabase();
        }

        public T GetData<T>(string key)
        {
            var value = _cacheDb.StringGet(key);
            if (!value.IsNull)
                return JsonSerializer.Deserialize<T>(value);

            return default;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            return _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
        }

        public bool RemoveData<T>(string key)
        {
            var exists = _cacheDb.KeyExists(key);

            if (exists)
                return _cacheDb.KeyDelete(key);

            return false;
        }
    }
}
