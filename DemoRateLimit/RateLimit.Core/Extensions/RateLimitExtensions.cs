using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateLimit.Core.BUS;
using RateLimit.Core.Configs;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RateLimit.Core.Extensions
{
    public static class RateLimitExtensions
    {
        public static void UseRateLimitMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RateLimitMiddleware>();
        }

        public static void AddRateLimit(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RateLimitConfig>(options => configuration.GetSection("RateLimit").Bind(options));
            services.AddRateLimitRegisStartup();
        }

        public static string GetIpAddressClient(this HttpContext context, bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                string ipAddressClient = string.Empty;
                if (System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Localhost")
                {
                    ipAddressClient = GetIpAddressServer();
                }
                else
                {
                    ipAddressClient = context.Request.Headers["X-Original-Forwarded-For"].FirstOrDefault() ??
                        context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                }
                return ipAddressClient;
            }
            return context.Connection.RemoteIpAddress != null ? context.Connection.RemoteIpAddress.ToString() : "";
        }

        public static string GetIpAddressServer()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipAddress.ToString();
                }
            }
            return string.Empty;
        }
    }
}
