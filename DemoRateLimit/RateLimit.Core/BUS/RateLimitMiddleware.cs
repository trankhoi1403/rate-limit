using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RateLimit.Core.BUS.Models;
using RateLimit.Core.Configs;
using RateLimit.Core.Extensions;
using RateLimit.Core.Services;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RateLimit.Core.BUS
{
    public class RateLimitMiddleware
    {
        readonly RequestDelegate _next;
        readonly RateLimitConfig _config;
        readonly ICache _cache;

        public RateLimitMiddleware(RequestDelegate next, IOptions<RateLimitConfig> option, ICache cache)
        {
            _next = next;
            _config = option.Value;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string cacheKey = GetKey(httpContext);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                var result = await Checking(httpContext, cacheKey);
                switch (result)
                {
                    case HttpStatusCode.Continue:
                        await _next(httpContext).ConfigureAwait(false);
                        break;
                    case HttpStatusCode.Accepted:
                        await _next(httpContext).ConfigureAwait(false);
                        break;
                    case HttpStatusCode.TooManyRequests:
                        httpContext.Response.StatusCode = (int)result;
                        await httpContext.Response.WriteAsync(httpContext.Response.StatusCode.ToString());
                        break;
                    case HttpStatusCode.UnavailableForLegalReasons:
                        httpContext.Response.StatusCode = (int)result;
                        await httpContext.Response.WriteAsync(httpContext.Response.StatusCode.ToString());
                        break;
                    default:
                        await _next(httpContext).ConfigureAwait(false);
                        break;
                }
                return;
            }

            await _next(httpContext);
        }

        async Task<HttpStatusCode> Checking(HttpContext httpContext, string cacheKey)
        {
            RequestIpHistoryModel ipHistoryModel = await _cache.GetAsync<RequestIpHistoryModel>(cacheKey);

            if (ipHistoryModel is null)
            {
                ipHistoryModel = new RequestIpHistoryModel()
                {
                    FirstApprovedRequest = DateTime.UtcNow,
                    NumberOfApprovedRequest = 1,
                    ReleaseTime = DateTime.MinValue,
                };
                //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 135 {Newtonsoft.Json.JsonConvert.SerializeObject(ipHistoryModel)}");
                var setResult = await _cache.SetAsync(cacheKey, ipHistoryModel);
                return setResult ? HttpStatusCode.Continue : HttpStatusCode.Accepted;
            }
            else
            {
                if (DateTime.UtcNow < ipHistoryModel.ReleaseTime)
                {
                    // return 429
                    //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 144 {HttpStatusCode.TooManyRequests}");
                    return HttpStatusCode.TooManyRequests;
                }
                else
                {
                    if ((DateTime.UtcNow - ipHistoryModel.FirstApprovedRequest).TotalSeconds <= _config.PeriodTime)
                    {
                        if (ipHistoryModel.NumberOfApprovedRequest < _config.MaxRequestPerPeriod)
                        {
                            ipHistoryModel.NumberOfApprovedRequest++;
                            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 153 {Newtonsoft.Json.JsonConvert.SerializeObject(ipHistoryModel)}");
                            var setResult = await _cache.SetAsync(cacheKey, ipHistoryModel);
                            return setResult ? HttpStatusCode.Continue : HttpStatusCode.Accepted;
                        }
                        else
                        {
                            //ipHistoryModel.NumberOfApprovedRequest = 0;
                            //ipHistoryModel.FirstApprovedRequest = default;
                            ipHistoryModel.ReleaseTime = DateTime.UtcNow.AddSeconds(_config.BlockTime);

                            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 163 {Newtonsoft.Json.JsonConvert.SerializeObject(ipHistoryModel)}");
                            var setResult = await _cache.SetAsync(cacheKey, ipHistoryModel);
                            return setResult ? HttpStatusCode.TooManyRequests : HttpStatusCode.Accepted;
                        }
                    }
                    else
                    {
                        ipHistoryModel.FirstApprovedRequest = DateTime.UtcNow;
                        ipHistoryModel.NumberOfApprovedRequest = 1;
                        ipHistoryModel.ReleaseTime = DateTime.MinValue;
                        //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 173 {Newtonsoft.Json.JsonConvert.SerializeObject(ipHistoryModel)}");
                        var setResult = await _cache.SetAsync(cacheKey, ipHistoryModel);
                        return setResult ? HttpStatusCode.Continue : HttpStatusCode.Accepted;
                    }
                }
            }
        }

        string GetKey(HttpContext httpContext)
        {
            var ip = httpContext.GetIpAddressClient();
            var requestPath = httpContext.Request.Path.Value?.Trim().ToLower() ?? string.Empty;
            if (_config.Requests.Find(e => requestPath == e.EndPoint) is RequestIdentity requestIdentity)
            {
                string headerValue = httpContext.Request.Headers[requestIdentity.HeaderKey];
                return $"request_id_{ip}_{requestPath}_{headerValue}";
            }
            return string.Empty;
        }
    }
}
