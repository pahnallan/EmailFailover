using Newtonsoft.Json;

namespace Ap.IntermediateEmailService.Models.SnailGun
{
    public class SnailGunRequest
    {
        [JsonProperty("from_email")]
        public string FromEmail { get; set; }
        [JsonProperty("from_name")]
        public string FromName { get; set; }
        [JsonProperty("to_email")]
        public string ToEmail { get; set; }
        [JsonProperty("to_name")]
        public string ToName { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
