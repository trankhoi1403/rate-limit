using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        [HttpGet("get-report-1")]
        public IActionResult GetReport_1()
        {
            //System.Threading.Thread.Sleep(3000);
            return Ok(nameof(GetReport_1));
        }

        [HttpGet("get-report-2")]
        public IActionResult GetReport_2()
        {
            //System.Threading.Thread.Sleep(3000);
            return Ok(nameof(GetReport_2));
        }
    }
}
