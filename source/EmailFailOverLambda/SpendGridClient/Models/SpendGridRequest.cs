using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpendGrid.Models
{
    public class SpendGridRequest
    {
        [JsonProperty("sender")]
        public string Sender { get; set; }
        [JsonProperty("recipient")]
        public string Recipient { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
