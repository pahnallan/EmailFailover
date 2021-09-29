using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Ap.IntermediateEmailService.Models.Interfaces;
using Ap.IntermediateEmailService.Services.Interfaces;
using Newtonsoft.Json;
using ReadSharp;
using RestSharp;

namespace Ap.IntermediateEmailService.Services
{
    public abstract class AbstractEmailApiService : IEmailService, IApiKeyAuthentication
    {
        public RestClient RestClient { get; set; }
        public IEmailMapper EmailMapper { get; set; }
        public string SecretManageApiKeyName { get; set; }

        // TODO: We could pull the api keys from secret manager rather than pulling from the lambda environment variables 
        public string ApiKey { get; set; }

        public virtual async Task<IRestResponse> SendEmailAsync(IEmailApiRequest emailApiRequest, ILambdaContext context, bool convertBodyToPlainText = true)
        {
            if (convertBodyToPlainText)
            {
                // Convert the body HTML to a plain text version
                emailApiRequest.Body = HtmlUtilities.ConvertToPlainText(emailApiRequest.Body);
            }

            var request = new RestRequest("", Method.POST, DataFormat.Json);
            var requestBody = EmailMapper.MapEmailApiRequest(emailApiRequest);
            context.Logger.LogLine($"Sending request with body {JsonConvert.SerializeObject(requestBody)}");
            request.AddParameter("application/json", JsonConvert.SerializeObject(requestBody), ParameterType.RequestBody);
            request.AddHeader("X-Api-Key", ApiKey);

            var response = await RestClient.ExecutePostAsync(request);
            context.Logger.LogLine($"Received response with where IsSuccessful = {response.IsSuccessful}");

            return response;
        }

        public virtual IRestResponse SendEmail(IEmailApiRequest emailApiRequest, ILambdaContext context, bool convertBodyToPlainText = true)
        {
            throw new NotImplementedException("Synchronous Email sending not implemented yet.");
        }
    }
}
