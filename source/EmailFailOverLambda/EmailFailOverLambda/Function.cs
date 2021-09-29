using System;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Ap.IntermediateEmailService.Models.IntermediateEmailClient;
using Ap.IntermediateEmailService.Services;
using Newtonsoft.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace EmailFailOverLambda
{
    public class Function
    {
        private readonly string _spendGridUrl = Environment.GetEnvironmentVariable("SpendGridUrlEndpoint");
        private readonly string _snailGunUrl = Environment.GetEnvironmentVariable("SnailGunUrlEndpoint");
        private readonly string _spendGridApiKey = Environment.GetEnvironmentVariable("SpendGridApiKey");
        private readonly string _snailGunApiKey = Environment.GetEnvironmentVariable("SnailGunApiKey");
        private readonly string _activeEmailProvider = Environment.GetEnvironmentVariable("ActiveEmailProvider");
        

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {

        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<EmailApiResponse> FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            // Limiting the batch messages to just 1 in the lambda batching for now. 
            foreach(var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }

            var response = new EmailApiResponse
            {
                RequestId = context.AwsRequestId,
                MessageId = evnt.Records.FirstOrDefault()?.MessageId
            };
            return response;
        }

        // No need to explicitly delete the message from the queue as AWS automatically deletes it when the lambda returns success
        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            try
            {
                var requestMessage = JsonConvert.DeserializeObject<EmailApiRequest>(message.Body);

                var emailService = new IntermediateEmailService(_spendGridUrl, _spendGridApiKey, _snailGunUrl, _snailGunApiKey, _activeEmailProvider);
                var response = await emailService.SendEmailAsync(requestMessage, context);
                if (!response.IsSuccessful)
                {
                    throw response.ErrorException;
                }


                context.Logger.LogLine($"{response.StatusCode},{response.Content}");
            }
            catch (Exception e)
            {
                context.Logger.LogLine(e.ToString());
                throw;
            }
        }
    }
}
