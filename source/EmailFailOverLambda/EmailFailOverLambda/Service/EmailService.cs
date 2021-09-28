using System.Threading.Tasks;
using EmailFailOverLambda.Models;
using Newtonsoft.Json;
using RestSharp;

namespace EmailFailOverLambda.Service
{
    public class EmailService : IEmailService, IApiKeyAuthentication
    {
        public RestClient RestClient { get; set; }
        public IEmailMapper EmailMapper { get; set; }
        public string SecretManageApiKeyName { get; set; }

        // TODO: Pull from secret manager instead of passing in by value directly.
        public string ApiKey { get; set; }

        public EmailService(string hostUrl, string apiKey, IEmailMapper mapper)
        {
            RestClient = new RestClient(hostUrl);
            ApiKey = apiKey;
            EmailMapper = mapper;
        }

        // TODO: Change return to email api response
        public async Task<IRestResponse> SendEmailAsync(IEmailApiRequest emailApiRequest)
        {
            var request = new RestRequest("", Method.POST, DataFormat.Json);
            var requestBody = EmailMapper.MapEmailApiRequest(emailApiRequest);
            request.AddParameter("application/json", JsonConvert.SerializeObject(requestBody), ParameterType.RequestBody);
            request.AddHeader("X-Api-Key", ApiKey);

            var response = await RestClient.ExecutePostAsync(request);

            return response;
        }
    }
}
