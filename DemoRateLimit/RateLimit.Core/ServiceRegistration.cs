using Microsoft.Extensions.DependencyInjection;
using RateLimit.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace RateLimit
{
    public static class ServiceRegistration
    {
        public static void AddRateLimitRegisStartup(this IServiceCollection services)
        {
            services.AddSingleton<ICache, RedisCache>();
        }
    }
}
