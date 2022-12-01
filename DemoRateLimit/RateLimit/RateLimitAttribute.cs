using RateLimit.BUS.Models;
using RateLimit.Configs;
using RateLimit.Services;
using System;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace RateLimit
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        readonly ICache _cache;
        readonly RateLimitConfig _config;

        /// <summary>
        /// A period of time (seconds) to checking
        /// <br/> Default: 10 seconds
        /// </summary>
        public int PeriodTime { get; set; } = 10;

        /// <summary>
        /// Max request in one period
        /// <br/> Default: 20 request
        /// </summary>
        public int MaxRequestPerPeriod { get; set; } = 1;

        /// <summary>
        /// Block a IP address in time.
        /// <br/> Default: 60 seconds
        /// </summary>
        public int BlockTime { get; set; } = 10;

        public RateLimitAttribute()
        {
            _cache = new RedisCache();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string cacheKey = GetKey(filterContext);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                var result = Checking(cacheKey);
                switch (result)
                {
                    case HttpStatusCode.Continue:
                        break;
                    case HttpStatusCode.Accepted:
                        break;
                    case HttpStatusCode.RequestTimeout:
                        filterContext.HttpContext.Response.StatusCode = 429;
                        filterContext.Result = new ContentResult
                        {
                            Content = "Too many request"
                        };
                        break;
                    default:
                        break;
                }
                return;
            }
        }

        HttpStatusCode Checking(string cacheKey)
        {
            RequestIpHistoryModel ipHistoryModel = _cache.Get<RequestIpHistoryModel>(cacheKey);

            if (ipHistoryModel is null)
            {
                ipHistoryModel = new RequestIpHistoryModel()
                {
                    FirstApprovedRequest = DateTime.UtcNow,
                    NumberOfApprovedRequest = 1,
                    ReleaseTime = DateTime.MinValue,
                };
                //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 135 {Newtonsoft.Json.JsonConvert.SerializeObject(ipHistoryModel)}");
                var setResult = _cache.Set(cacheKey, ipHistoryModel);
                return setResult ? HttpStatusCode.Continue : HttpStatusCode.Accepted;
            }
            else
            {
                if (DateTime.UtcNow < ipHistoryModel.ReleaseTime)
                {
                    // return 429
                    //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 144 {HttpStatusCode.TooManyRequests}");
                    return HttpStatusCode.RequestTimeout;
                }
                else
                {
                    if ((DateTime.UtcNow - ipHistoryModel.FirstApprovedRequest).TotalSeconds <= PeriodTime)
                    {
                        if (ipHistoryModel.NumberOfApprovedRequest < MaxRequestPerPeriod)
                        {
                            ipHistoryModel.NumberOfApprovedRequest++;
                            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 153 {Newtonsoft.Json.JsonConvert.SerializeObject(ipHistoryModel)}");
                            var setResult = _cache.Set(cacheKey, ipHistoryModel);
                            return setResult ? HttpStatusCode.Continue : HttpStatusCode.Accepted;
                        }
                        else
                        {
                            //ipHistoryModel.NumberOfApprovedRequest = 0;
                            //ipHistoryModel.FirstApprovedRequest = default;
                            ipHistoryModel.ReleaseTime = DateTime.UtcNow.AddSeconds(BlockTime);

                            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 163 {Newtonsoft.Json.JsonConvert.SerializeObject(ipHistoryModel)}");
                            var setResult = _cache.Set(cacheKey, ipHistoryModel);
                            return setResult ? HttpStatusCode.RequestTimeout : HttpStatusCode.Accepted;
                        }
                    }
                    else
                    {
                        ipHistoryModel.FirstApprovedRequest = DateTime.UtcNow;
                        ipHistoryModel.NumberOfApprovedRequest = 1;
                        ipHistoryModel.ReleaseTime = DateTime.MinValue;
                        //System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("mm:ss.ffffff")} : 173 {Newtonsoft.Json.JsonConvert.SerializeObject(ipHistoryModel)}");
                        var setResult = _cache.Set(cacheKey, ipHistoryModel);
                        return setResult ? HttpStatusCode.Continue : HttpStatusCode.Accepted;
                    }
                }
            }
        }

        string GetKey(ActionExecutingContext filterContext)
        {
            var key = string.Format("request_id_{0}-{1}-{2}",
               filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
               filterContext.ActionDescriptor.ActionName,
               filterContext.HttpContext.Request.UserHostAddress
               );
            return key;
        }
    }
}