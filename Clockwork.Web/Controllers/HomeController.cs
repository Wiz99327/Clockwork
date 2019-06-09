using Clockwork.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Clockwork.Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var endpointUrl = ConfigurationManager.AppSettings["ClockworkApiEndpoint"];
            ViewData["ClockworkEndpointUrl"] = endpointUrl;
            var mvcName = typeof(Controller).Assembly.GetName();
            var isMono = Type.GetType("Mono.Runtime") != null;

            ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
            ViewData["Runtime"] = isMono ? "Mono" : ".NET";

            HttpClient httpClient = new HttpClient();
            try
            {
                var timeZoneTask = httpClient.GetAsync($"{endpointUrl}/api/currenttime/supportedTimeZones");
                var historyTask = httpClient.GetAsync($"{endpointUrl}/api/currenttime/history");

                Task.WaitAll(timeZoneTask, historyTask);

                var timeZoneRsp = await timeZoneTask.ConfigureAwait(false);
                var historyRsp = await historyTask.ConfigureAwait(false);

                Task<string> timeZoneContentTask = null;
                if (timeZoneRsp?.Content != null)
                {
                    timeZoneContentTask = timeZoneRsp.Content.ReadAsStringAsync();
                }

                Task<string> timeHistoryContentTask = null;
                if (historyRsp?.Content != null)
                {
                    timeHistoryContentTask = historyRsp.Content.ReadAsStringAsync();
                }

                Task.WaitAll(timeZoneContentTask, timeHistoryContentTask);

                var supportedTimeZones = await timeZoneContentTask.ConfigureAwait(false);
                var timeHistory = await timeHistoryContentTask.ConfigureAwait(false);

                ViewData["SupportedTimeZones"] = JsonConvert.DeserializeObject<List<TimeZoneInfo>>(supportedTimeZones);
                ViewData["CurrentTimeZone"] = TimeZoneInfo.Local.DisplayName;
                ViewData["TimeHistory"] = JsonConvert.DeserializeObject<List<ClockworkDisplayModel>>(timeHistory);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred on page load: ", ex.StackTrace);
            }

            return View();
        }
    }
}
