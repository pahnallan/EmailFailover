using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EmailFailOverLambda.Models
{
    public class EmailApiRequest : IEmailApiRequest
    {
        [JsonProperty("to")]
        public string To { get; set; }
        [JsonProperty("to_name")]
        public string ToName { get; set; }
        [JsonProperty("from")]
        public string From { get; set; }
        [JsonProperty("from_name")]
        public string FromName { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
