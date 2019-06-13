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
                var timeZoneRsp = await httpClient.GetAsync($"{endpointUrl}/api/currenttime/supportedTimeZones").ConfigureAwait(false);
                var historyRsp = await httpClient.GetAsync($"{endpointUrl}/api/currenttime/history").ConfigureAwait(false);

                if (timeZoneRsp?.Content != null)
                {
                    var supportedTimeZones = await timeZoneRsp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ViewData["SupportedTimeZones"] = JsonConvert.DeserializeObject<List<TimeZoneInfo>>(supportedTimeZones);
                }

                ViewData["CurrentTimeZone"] = TimeZoneInfo.Local.DisplayName;

                if (historyRsp?.Content != null)
                {
                    var timeHistory = await historyRsp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ViewData["TimeHistory"] = JsonConvert.DeserializeObject<List<ClockworkDisplayModel>>(timeHistory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred on page load: ", ex.StackTrace);
            }

            return View();
        }
    }
}
