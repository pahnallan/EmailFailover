using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;
using EmailFailOverLambda.Models;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace EmailFailOverLambda.Tests
{
    public class FunctionTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public FunctionTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            // credentials.txt should contain 2 lines, first line is the api key for SpendGrid, second line is the api key for SnailGun
            var credentialList = File.ReadAllLines("credentials.txt");
            Environment.SetEnvironmentVariable("SpendGridUrlEndpoint", "https://bw-interviews.herokuapp.com/spendgrid/send_email");
            Environment.SetEnvironmentVariable("SnailGunUrlEndpoint", "https://bw-interviews.herokuapp.com/snailgun/emails");
            Environment.SetEnvironmentVariable("SpendGridApiKey", credentialList[0]);
            Environment.SetEnvironmentVariable("SnailGunApiKey", credentialList[1]);
            Environment.SetEnvironmentVariable("ActiveEmailProvider", "SpendGrid");
        }

        [Fact]
        public async Task TestSQSEventLambdaFunctionSpendGrid()
        {
            Environment.SetEnvironmentVariable("ActiveEmailProvider", "SpendGrid");
            var emailApiRequest = new EmailApiRequest
            {
                To = "susan@abcpreschool.org",
                ToName = "Miss Susan",
                From = "theallanp@gmail.com",
                FromName = "Allan P",
                Subject = "Your Daily Test Email",
                Body = "<h1>Hello World</h1>"
            };

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = JsonConvert.SerializeObject(emailApiRequest)
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            var logBuffer = logger.Buffer.ToString();
            _testOutputHelper.WriteLine(logBuffer);
            Assert.Contains($"{emailApiRequest.From}", logBuffer);
            Assert.Contains($"{emailApiRequest.To}", logBuffer);
            Assert.Contains($"Created", logBuffer);
        }

        [Fact]
        public async Task TestSQSEventLambdaFunctionSnailGun()
        {
            Environment.SetEnvironmentVariable("ActiveEmailProvider", "SnailGun");
            var emailApiRequest = new EmailApiRequest
            {
                To = "susan@abcpreschool.org",
                ToName = "Miss Susan",
                From = "theallanp@gmail.com",
                FromName = "Allan P",
                Subject = "Your Daily Test Email",
                Body = "<h1>Hello World</h1>"
            };

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = JsonConvert.SerializeObject(emailApiRequest)
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            var logBuffer = logger.Buffer.ToString();
            _testOutputHelper.WriteLine(logBuffer);
            Assert.Contains($"queued", logBuffer);
            Assert.Contains($"OK", logBuffer);
        }
    }
}
