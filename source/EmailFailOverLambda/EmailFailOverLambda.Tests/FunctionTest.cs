using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;
using Ap.IntermediateEmailService.Models.IntermediateEmailClient;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace EmailFailOverLambda.Tests
{
    public class FunctionTest
    {
        // TODO: Add Mock Email Provider to test against.
        // TODO: Add unit tests for new synchronous handler in lambda (include testing of response codes)
        private readonly ITestOutputHelper _testOutputHelper;
        private TestLambdaLogger TestLogger { get; set; }
        private TestLambdaContext TestContext { get; set; }

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



            TestLogger = new TestLambdaLogger();
            TestContext = new TestLambdaContext
            {
                Logger = TestLogger
            };
        }

        [Fact]
        public async Task TestSpendGrid()
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

            var function = new Function();
            await function.FunctionHandler(sqsEvent, TestContext);

            var logBuffer = TestLogger.Buffer.ToString();
            _testOutputHelper.WriteLine(logBuffer);

            Assert.Contains($"{emailApiRequest.From}", logBuffer);
            Assert.Contains($"{emailApiRequest.To}", logBuffer);
            Assert.Contains($"Created", logBuffer);
        }

        [Fact]
        public async Task TestSnailGun()
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

            var function = new Function();
            await function.FunctionHandler(sqsEvent, TestContext);

            var logBuffer = TestLogger.Buffer.ToString();
            _testOutputHelper.WriteLine(logBuffer);

            Assert.Contains($"queued", logBuffer);
            Assert.Contains($"OK", logBuffer);
        }


        [Fact]
        public async Task TestHtmlConversion()
        {
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

            var function = new Function();
            await function.FunctionHandler(sqsEvent, TestContext);

            var logBuffer = TestLogger.Buffer.ToString();
            _testOutputHelper.WriteLine(logBuffer);

            Assert.DoesNotContain("<h1>", logBuffer);
            Assert.DoesNotContain("</h1>", logBuffer);
        }

        [Fact]
        public  void TestMissingRequestFields()
        {
            // AWS API Gateway is checking for the required fields.
            // Currently can't test missing request fields due to missing fields never entering the code. 
        }
    }
}
