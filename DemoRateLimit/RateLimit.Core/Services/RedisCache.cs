using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RateLimit.Core.Configs;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimit.Core.Services
{
    public class RedisCache : ICache
    {
        private readonly ConnectionMultiplexer _redis;
        readonly RateLimitConfig _config;
        IServer _server;
        IDatabase _database;

        public RedisCache(IOptions<RateLimitConfig> option)
        {
            try
            {
                _config = option?.Value;
                if (_config != null && _config.CacheProvider != null && _config.CacheProvider.Redis != null)
                {
                    _redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
                    {
                        EndPoints = { _config.CacheProvider.Redis.Host },
                        ConnectTimeout = 4000,
                        AbortOnConnectFail = true,
                    });
                    if (_redis != null)
                    {
                        var endpoint = _redis.GetEndPoints();
                        _server = _redis.GetServer(endpoint.First());
                        _database = _redis?.GetDatabase(_config.CacheProvider.Redis.Database);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss.ffffff")} {nameof(RedisCache)}: {JsonConvert.SerializeObject(ex)}");
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (_redis?.IsConnected ?? false)
            {
                if ((await _database.StringGetAsync(key)) is RedisValue value && !value.IsNullOrEmpty)
                {
                    return JsonConvert.DeserializeObject<T>(value.ToString());
                }
            }
            return default(T);
        }

        public async Task<bool> SetAsync<T>(string key, T item)
        {
            if (_redis?.IsConnected ?? false)
            {
                var strItem = JsonConvert.SerializeObject(item);
                return await _database.StringSetAsync(key, strItem);
            }
            return false;
        }
    }
}
