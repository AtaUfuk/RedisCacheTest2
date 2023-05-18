using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace RedisCacheTest2.Lib.Cache
{
    public class DistributedRedisCache : IDistributedRedisCache
    {
        #region Properties
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private double _expireNumber { get; set; }
        private string _expireType { get; set; }
        private double _slidingExpireNumber { get; set; }
        private string instanceName { get; set; }
        #endregion
        public DistributedRedisCache(IDistributedCache cache)
        {
            _cache = cache;
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false);
            _configuration = builder.Build();
            _expireNumber = double.Parse(_configuration.GetSection("MemoryCacheOptions:ExpireNumber").Value ?? "30");
            _slidingExpireNumber = double.Parse(_configuration.GetSection("MemoryCacheOptions:SlidingExpireNumber").Value ?? "30");
            _expireType = _configuration.GetSection("MemoryCacheOptions:ExpireDurationType").Value ?? "D";
            instanceName = _configuration.GetSection("MemoryCacheOptions:InstanceName").Value ?? string.Empty;
        }
        #region Public Methods
        public void ClearAll()
        {

            var connection = _configuration.GetConnectionString("Redis");
            var _connectionMultiplexer = ConnectionMultiplexer.Connect(connection + ",allowAdmin=true");
            var endpoint = _connectionMultiplexer.GetEndPoints(true).First();
            var server = _connectionMultiplexer.GetServer(endpoint);
            server.FlushAllDatabases();
        }

        public async Task<T?> GetRecordAsync<T>(string recordId)
        {
            var jsonData = await _cache.GetStringAsync(recordId);
            if (object.Equals(jsonData, null))
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public async Task Remove(string recordId)
        {
            var connection = _configuration.GetConnectionString("Redis");
            var connectionMultiplexer = ConnectionMultiplexer.Connect(connection);
            IDatabase cacheDB = connectionMultiplexer.GetDatabase();
            await cacheDB.KeyDeleteAsync(recordId);
        }

        public async Task RemoveAllKeys()
        {

            var connection = _configuration.GetConnectionString("Redis");
            var connectionMultiplexer = ConnectionMultiplexer.Connect(connection);
            var endpoint = connectionMultiplexer.GetEndPoints(true).First();
            var server = connectionMultiplexer.GetServer(endpoint);
            string pattern = $"{instanceName}*";
            var keys = server.Keys(pattern: pattern);
            RedisKey[] redisKeys = keys.ToArray();
            IDatabase cacheDB = connectionMultiplexer.GetDatabase();
            foreach (var key in redisKeys)
            {
                await cacheDB.KeyDeleteAsync(key); /*Worked*/
                /*await Remove(key.ToString().Replace(instanceName,"")); /*not worked*/
                /*await Remove(key);/* not worked*/
            }
        }

        public async Task SetorUpdateRecordAsync<T>(string recordId, T data)
        {
            if (KeyIsExists(recordId))
            {
                _cache.Remove(recordId);
            }
            var options = CreateDistributedCacheOptions();
            var jsonData = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(recordId, jsonData, options);
        }

        public async Task SetRecordAsync<T>(string recordId, T data)
        {
            var options = CreateDistributedCacheOptions();
            var jsonData = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(recordId, jsonData, options);
        }
        #endregion
        #region Private Methods
        private bool KeyIsExists(string recorId)
        {
            return !string.IsNullOrWhiteSpace(_cache.GetString(recorId));
        }
        private DistributedCacheEntryOptions CreateDistributedCacheOptions()
        {
            TimeSpan absoluteExpireTime = TimeSpan.FromSeconds(1);
            TimeSpan slidingExpiration = TimeSpan.FromSeconds(1);
            if (_expireType == "S")
            {
                absoluteExpireTime = TimeSpan.FromSeconds(_expireNumber);
                slidingExpiration = TimeSpan.FromSeconds(_slidingExpireNumber);
            }
            else if (_expireType == "Min")
            {
                absoluteExpireTime = TimeSpan.FromMinutes(_expireNumber);
                slidingExpiration = TimeSpan.FromMinutes(_slidingExpireNumber);
            }
            else if (_expireType == "D")
            {
                absoluteExpireTime = TimeSpan.FromDays(_expireNumber);
                slidingExpiration = TimeSpan.FromDays(_slidingExpireNumber);
            }
            else if (_expireType == "M")
            {
                DateTime now = DateTime.Now;
                absoluteExpireTime = DateTime.Now.AddMonths((int)_expireNumber) - now;
                slidingExpiration = DateTime.Now.AddMonths((int)_slidingExpireNumber) - now;
            }
            else if (_expireType == "Y")
            {
                DateTime now = DateTime.Now;
                absoluteExpireTime = DateTime.Now.AddYears((int)_expireNumber) - now;
                slidingExpiration = DateTime.Now.AddYears((int)_slidingExpireNumber) - now;
            }
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime,
                SlidingExpiration = slidingExpiration,
            };
            return options;
        }
        #endregion
    }
}
