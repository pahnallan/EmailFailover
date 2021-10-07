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
            var message = evnt.Records.FirstOrDefault();
            var requestMessage = JsonConvert.DeserializeObject<EmailApiRequest>(message.Body);
            var response = await ProcessMessageAsync(requestMessage, context);
            // TODO: Add logging to response?
            return response;
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="emailApiRequest"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<EmailApiResponse> FunctionHandler(EmailApiRequest emailApiRequest, ILambdaContext context)
        {
            var response = await ProcessMessageAsync(emailApiRequest, context);
            // TODO: Add logging to response?
            return response;
        }

        // No need to explicitly delete the message from the queue as AWS automatically deletes it when the lambda returns success
        private async Task<EmailApiResponse> ProcessMessageAsync(EmailApiRequest requestMessage, ILambdaContext context)
        {
            try
            {
                var validationErrors = IntermediateEmailService.ValidateEmailApiRequest(requestMessage);
                if (validationErrors.Any())
                {
                    return new EmailApiResponse
                    {
                        Status = "BadRequest",
                        Message = string.Join(" ", validationErrors),
                        RequestId = context.AwsRequestId
                    };
                }

                var emailService = new IntermediateEmailService(_spendGridUrl, _spendGridApiKey, _snailGunUrl, _snailGunApiKey, _activeEmailProvider);
                var response = await emailService.SendEmailAsync(requestMessage, context);
                if (!response.IsSuccessful)
                {
                    // Third party email provider was not reachable so our api is also unavailable.
                    return new EmailApiResponse
                    {
                        Status = "ServiceUnavailable",
                        Message = response.ErrorException.Message,
                        RequestId = context.AwsRequestId
                    };
                }

                context.Logger.LogLine($"{response.StatusCode},{response.Content}");

                return new EmailApiResponse
                {
                    Status = "Success",
                    Message = "Successfully Sent Email.",
                    RequestId = context.AwsRequestId
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine(e.ToString());

                return new EmailApiResponse
                {
                    Status = "InternalError",
                    Message = "The server ran into an error while processing the request.",
                    RequestId = context.AwsRequestId
                };
            }
        }
    }
}
