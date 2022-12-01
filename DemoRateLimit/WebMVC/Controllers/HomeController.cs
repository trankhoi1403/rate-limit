using Newtonsoft.Json;
using RateLimit;
using RateLimit.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebMVC.Controllers
{
    public class HomeController : Controller
    {
        RateLimitConfig config = new RateLimitConfig() { };

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Demo rate limit";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [RateLimit(PeriodTime = 10, MaxRequestPerPeriod = 1, BlockTime = 10)]
        public string GetReport(string param1)
        {
            var reportData = new
            {
                Action = nameof(GetReport),
                Data = param1,
            };
            string reportJson = JsonConvert.SerializeObject(reportData);

            return reportJson;
        }
    }
}