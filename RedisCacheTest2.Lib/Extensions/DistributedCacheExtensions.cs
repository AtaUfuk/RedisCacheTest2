using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace RedisCacheTest2.Lib.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task SetorUpdateRecordAsync<T>(this IDistributedCache cache, string recordId, T data, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60),
                SlidingExpiration = unusedExpireTime
            };
            if (IsExistRecord(cache, recordId))
            {
                cache.Remove(recordId);
            }
            var jsonData = JsonSerializer.Serialize(data);
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static async Task<T?> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
        {
            var jsonData = await cache.GetStringAsync(recordId);
            if (object.Equals(jsonData, null))
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public static bool IsExistRecord(IDistributedCache cache, string recordId)
        {
            var jsonData = cache.GetString(recordId);
            bool result = false;
            if (!object.Equals(jsonData, null))
            {
                result = true;
            }
            return result;
        }

        public static async Task SetRecordAsync<T>(this IDistributedCache cache, string recordId, T data, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60),
                SlidingExpiration = unusedExpireTime
            };
            var jsonData = JsonSerializer.Serialize(data);
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static void ClearAll(this IDistributedCache cache)
        {

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false);
            IConfiguration configuration = builder.Build();
            var connection = configuration.GetConnectionString("Redis");
            var _connectionMultiplexer = ConnectionMultiplexer.Connect(connection+ ",allowAdmin=true");
            var endpoint = _connectionMultiplexer.GetEndPoints(true).First();
            var server = _connectionMultiplexer.GetServer(endpoint);
            server.FlushAllDatabases();
        }
    }
}
