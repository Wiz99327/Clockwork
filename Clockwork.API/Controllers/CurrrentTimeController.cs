using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Clockwork.API.Models;

namespace Clockwork.API.Controllers
{
    [Route("api/[controller]")]
    public class CurrentTimeController : Controller
    {
        // GET api/currenttime
        [HttpGet]
        public IActionResult Get([FromQuery] string timeZoneId)
        {
            var utcTime = DateTime.UtcNow;
            var serverTime = DateTimeOffset.Now;
            if (!string.IsNullOrWhiteSpace(timeZoneId))
            {
                var timeZones = TimeZoneInfo.GetSystemTimeZones();
                var timeZone = timeZones.FirstOrDefault(x => x.Id.Equals(timeZoneId, StringComparison.OrdinalIgnoreCase));
                if (timeZone == null)
                {
                    Console.WriteLine("Invalid or Unsupported Time Zone Id: {0}", timeZoneId);
                    return BadRequest();
                }

                serverTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, timeZone.Id);
            }

            var ip = this.HttpContext.Connection.RemoteIpAddress.ToString();

            var returnVal = new CurrentTimeQuery
            {
                UTCTime = utcTime,
                ClientIp = ip,
                Time = serverTime
            };

            using (var db = new ClockworkContext())
            {
                db.CurrentTimeQueries.Add(returnVal);
                var count = db.SaveChanges();
                Console.WriteLine("{0} records saved to database", count);

                Console.WriteLine();
                foreach (var CurrentTimeQuery in db.CurrentTimeQueries)
                {
                    Console.WriteLine(" - {0}", CurrentTimeQuery.UTCTime);
                }
            }

            return Ok(returnVal);
        }

        [HttpGet]
        [Route("history")]
        public IActionResult GetHistory()
        {
            List<CurrentTimeQuery> timeQueryHistory = null;
            using (var db = new ClockworkContext())
            {
                Console.WriteLine("Retrieving time query history from database");

                if (!db.CurrentTimeQueries.Any())
                {
                    Console.WriteLine("No previous time records found in database");
                    return NoContent();
                }

                timeQueryHistory = db.CurrentTimeQueries.ToList();

                Console.WriteLine("{0} records found in database", timeQueryHistory.Count);
            }

            return Ok(timeQueryHistory);
        }

        [HttpGet]
        [Route("supportedTimeZones")]
        public IActionResult GetSupportedTimeZones()
        {
            return Ok(TimeZoneInfo.GetSystemTimeZones());
        }
    }
}
