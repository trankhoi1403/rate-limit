using Newtonsoft.Json;
using RateLimit.Configs;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimit.Services
{
    public class RedisCache : ICache
    {
        private readonly ConnectionMultiplexer _redis;
        readonly RedisConfig _config = new RedisConfig();
        IServer _server;
        IDatabase _database;

        public RedisCache()
        {
            try
            {
                if (_config != null )
                {
                    _redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
                    {
                        EndPoints = { _config.Host },
                        ConnectTimeout = _config.ConnectTimeout,
                        AbortOnConnectFail = true,
                    });
                    if (_redis != null)
                    {
                        var endpoint = _redis.GetEndPoints();
                        _server = _redis.GetServer(endpoint.First());
                        _database = _redis?.GetDatabase(_config.Database);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss.ffffff")} {nameof(RedisCache)}: {JsonConvert.SerializeObject(ex)}");
            }
        }

        public T Get<T>(string key)
        {
            if (_redis?.IsConnected ?? false)
            {
                if (_database.StringGet(key) is RedisValue value && !value.IsNullOrEmpty)
                {
                    return JsonConvert.DeserializeObject<T>(value.ToString());
                }
            }
            return default(T);
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

        public bool Set<T>(string key, T item)
        {
            if (_redis?.IsConnected ?? false)
            {
                var strItem = JsonConvert.SerializeObject(item);
                return _database.StringSet(key, strItem);
            }
            return false;
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
