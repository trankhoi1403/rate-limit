using System.Collections.Generic;

namespace RateLimit.Core.Configs
{
    public class RateLimitConfig
    {
        public CacheProvider CacheProvider { get; set; } = new CacheProvider();

        /// <summary>
        /// A period of time (seconds) to checking
        /// <br/> Default: 10 seconds
        /// </summary>
        public int PeriodTime { get; set; } = 10;

        /// <summary>
        /// Max request in one period
        /// <br/> Default: 20 request
        /// </summary>
        public int MaxRequestPerPeriod { get; set; } = 20;

        /// <summary>
        /// Block a IP address in time.
        /// <br/> Default: 60 seconds
        /// </summary>
        public int BlockTime { get; set; } = 60;

        /// <summary>
        /// List end point to check
        /// </summary>
        public List<RequestIdentity> Requests { get; set; } = new List<RequestIdentity>();
    }

    public class CacheProvider
    {
        public RedisConfig Redis { get; set; } = null;
        public MemoryConfig Memory { get; set; } = null;
    }

    public abstract class CacheConfig
    {
        public int CacheTime { get; set; }
    }

    public class RedisConfig : CacheConfig
    {
        public string Host { get; set; }
        public int Database { get; set; }
    }
    
    public class MemoryConfig : CacheConfig
    {
    }

    public class RequestIdentity
    {
        /// <summary>
        /// End point to identify a request. Ex: /controller/feature-1
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// Header key to identify a request. Ex: "ServiceName"
        /// </summary>
        public string HeaderKey { get; set; }
    }
}
