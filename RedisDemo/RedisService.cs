using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisDemo.Models;
using StackExchange.Redis;
using System.Text.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RedisDemo
{
    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IServer _server;
        private readonly ILogger<RedisService> _logger;
        private readonly IDatabase _db;
        private object _locker = new object();

        public RedisService(ILogger<RedisService> logger, IOptions<RedisConfiguration> options)
        {
            _logger = logger;
            var endpoint = options.Value.Endpoint;
            _redis = ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                AbortOnConnectFail = false,
                EndPoints = new EndPointCollection()
                {
                    endpoint
                },
                AllowAdmin = true,
            });
            _server = _redis.GetServer(endpoint);
            _db = _redis.GetDatabase();
        }

        public async Task TestDB()
        {
            var db = _redis.GetDatabase();
            var pong = await db.PingAsync();
            Console.WriteLine(pong);
        }

        public IEnumerable<string> GetAllKeysAsync()
        {
            var allKeys = _server.Keys().Select(x => (string)x);
            return allKeys;
        }

        public bool InsertDbEntities<T>(string keyName, T obj) where T : class
        {
            lock (_locker)
            {
                return _db.StringSet(keyName, JsonSerializer.Serialize(obj));
            }
        }

        public async Task<bool> FastInsertDbEntities<T>(string keyName, T obj) where T : class
        {   
            return await _db.StringSetAsync(keyName, JsonSerializer.Serialize(obj));
        }

        public async Task<T> GetDbEntities<T>(string keyName) where T : class
        {
            var response = await _db.StringGetAsync(keyName);
            if (!string.IsNullOrWhiteSpace(response))
                return JsonSerializer.Deserialize<T>(response);
            else return null;
        }

        public bool DeleteDbEntities(string keyName)
        {
            lock (_locker)
            {
                return _db.StringSet(keyName, RedisValue.Null);
            }
        }

        public async IAsyncEnumerable<T> GetAllDbEntities<T>() where T : class
        {
            var keys = _server.KeysAsync();
            await foreach (var key in keys)
            {
                var response = await _db.StringGetAsync(key);
                yield return JsonSerializer.Deserialize<T>(response);
            }
        }

        public void DeleteAllKeys()
        {
            lock (_locker)
            {
                _server.FlushDatabase();
            }
        }
    }
}