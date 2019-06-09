using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Clockwork.Web.Models
{
    [JsonObject]
    public class ClockworkDisplayModel
    {
        [JsonProperty]
        public int CurrentTimeQueryId { get; set; }
        [JsonProperty]
        public string Time { get; set; }
        [JsonProperty]
        public string UtcTime { get; set; }
        [JsonProperty]
        public string ClientIp { get; set; }
    }
}