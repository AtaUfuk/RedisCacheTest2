﻿using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisCacheTest2.Lib.Cache
{
    public class RedisCacheMem : IRedisCacheMem
    {
        private RedisCacheOptions _redisOptions { get; set; }
        private ConnectionMultiplexer _connectionMultiplexer { get; set; }
        private IDatabase _cache { get; set; }
        private double _expireNumber { get; set; }
        private string expireType { get; set; }
        private string _instanceName { get; set; }

        public RedisCacheMem()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false);
            IConfiguration configuration = builder.Build();
            _redisOptions = new()
            {
                Configuration = configuration.GetConnectionString("Redis") + ",allowAdmin=true"
            };
            _connectionMultiplexer = ConnectionMultiplexer.Connect(_redisOptions.Configuration);
            _cache = _connectionMultiplexer.GetDatabase();
            _instanceName = configuration.GetSection("MemoryCacheOptions:InstanceName").Value ?? string.Empty;
            _expireNumber = double.Parse(configuration.GetSection("MemoryCacheOptions:ExpireNumber").Value ?? "30");
            expireType = configuration.GetSection("MemoryCacheOptions:ExpireDurationType").Value ?? "D";
        }

        public void ClearAll()
        {
            var endpoint = _connectionMultiplexer.GetEndPoints(true).First();
            var server = _connectionMultiplexer.GetServer(endpoint);
            server.FlushDatabase();
        }

        public async Task<T?> GetRecordAsync<T>(string recordId)
        {
            var jsonData = await _cache.StringGetAsync(_instanceName + recordId);
            if (!jsonData.HasValue)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public async Task SetorUpdateRecordAsync<T>(string recordId, T data)
        {
            TimeSpan ts = TimeSpan.FromDays(1);
            if (_cache.KeyExists(_instanceName + recordId))
            {
                _cache.KeyDelete(_instanceName + recordId);
            }
            var jsonData = JsonSerializer.Serialize(data);
            if (expireType == "S")
            {
                ts = TimeSpan.FromSeconds(_expireNumber);
            }
            else if (expireType == "Min")
            {
                ts = TimeSpan.FromMinutes(_expireNumber);
            }
            else if (expireType == "D")
            {
                ts = TimeSpan.FromDays(_expireNumber);
            }
            else if (expireType == "M")
            {
                ts = TimeSpan.FromDays(_expireNumber * 30);
            }
            else if (expireType == "Y")
            {
                ts = TimeSpan.FromDays(_expireNumber * 365);
            }
            await _cache.StringSetAsync(_instanceName + recordId, jsonData, ts);
        }

        public async Task SetRecordAsync<T>(string recordId, T data)
        {
            var jsonData = JsonSerializer.Serialize(data);
            TimeSpan ts = TimeSpan.FromDays(1);
            if (expireType == "S")
            {
                ts = TimeSpan.FromSeconds(_expireNumber);
            }
            else if (expireType == "Min")
            {
                ts = TimeSpan.FromMinutes(_expireNumber);
            }
            else if (expireType == "D")
            {
                ts = TimeSpan.FromDays(_expireNumber);
            }
            else if (expireType == "M")
            {
                ts = TimeSpan.FromDays(_expireNumber * 30);
            }
            else if (expireType == "Y")
            {
                ts = TimeSpan.FromDays(_expireNumber * 365);
            }
            await _cache.StringSetAsync(_instanceName + recordId, jsonData, ts);
        }
    }
}
