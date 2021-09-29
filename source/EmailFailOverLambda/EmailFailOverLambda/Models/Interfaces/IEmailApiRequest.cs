using Newtonsoft.Json;

namespace EmailFailOverLambda.Models
{
    public interface IEmailApiRequest
    {
        [JsonProperty("to")]
        string To { get; set; }
        [JsonProperty("to_name")]
        string ToName { get; set; }
        [JsonProperty("from")]
        string From { get; set; }
        [JsonProperty("from_name")]
        string FromName { get; set; }
        [JsonProperty("subject")]
        string Subject { get; set; }
        [JsonProperty("body")]
        string Body { get; set; }
    }
}
