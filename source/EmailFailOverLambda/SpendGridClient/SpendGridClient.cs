using System;
using Newtonsoft.Json;
using RestSharp;
using SpendGrid.Models;

namespace SpendGrid
{
    public class SpendGridClient
    {
        private RestClient RestClient { get; set; }

        // TODO: Pull from secret manager instead of passing in by value directly.
        private string SecretManagerApiKeyName { get; set; }
        private string ApiKey { get; set; }


        public SpendGridClient(string hostUrl, string apiKey)
        {
            RestClient = new RestClient(hostUrl);
            ApiKey = apiKey;
        }

        public IRestResponse SendEmail(SpendGridRequest spendGridRequest)
        {
            var request = new RestRequest("", Method.POST, DataFormat.Json);
            request.AddParameter("application/json", JsonConvert.SerializeObject(spendGridRequest), ParameterType.RequestBody);
            request.AddHeader("X-Api-Key", ApiKey);

            var response = RestClient.Post(request);

            return response;
        }

        public IRestResponse SendEmail(string sender, string recipient, string subject, string body)
        {
            var spendGridRequest = new SpendGridRequest()
            {
                Sender = sender,
                Recipient = recipient,
                Subject = subject,
                Body = body
            };

            return SendEmail(spendGridRequest);
        }
    }
}
