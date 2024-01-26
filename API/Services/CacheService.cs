using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks; 

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

        public bool RemoveData(string key)
        {
            var exists = _cacheDb.KeyExists(key);

            if (exists)
                return _cacheDb.KeyDelete(key);

            return false;
        }

        public async Task<bool> GetRequest<T>(string key)
        {
            var cacheData = GetData<IEnumerable<T>>(key);

            if (cacheData != null && cacheData.Any())
                return true;

            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            SetData(key, cacheData, expiryTime);

            return true;
        }

        public void PostRequest<T>(string key, int id, T value)
        {
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            SetData($"{key}{id}", value, expiryTime);
        }

        public void DeleteRequest(string key, int id)
        {
            RemoveData($"{key}{id}");
        }
    }
}
