using Ap.IntermediateEmailService.Models.IntermediateEmailClient;
using Ap.IntermediateEmailService.Models.SnailGun;
using Ap.IntermediateEmailService.Models.SpendGrid;
using Xunit;

namespace Ap.IntermediateEmailService.Tests
{
    public class MappingTests
    {
        private EmailApiRequest InputRequest { get; set; }

        public MappingTests()
        {
            InputRequest = new EmailApiRequest
            {
                To = "susan@abcpreschool.org",
                ToName = "Miss Susan",
                From = "theallanp@gmail.com",
                FromName = "Allan P",
                Subject = "Your Daily Test Email",
                Body = "<h1>Hello World</h1>"
            };
        }

        [Fact]
        public void TestSpendGridMapping()
        {
            var spendGridMapper = new SendGridMapper();
            var outputRequest = spendGridMapper.MapEmailApiRequest(InputRequest) as SpendGridRequest;

            Assert.NotNull(outputRequest);
            Assert.Equal("Allan P <theallanp@gmail.com>", outputRequest.Sender);
            Assert.Equal("Miss Susan <susan@abcpreschool.org>", outputRequest.Recipient);
            Assert.Equal("Your Daily Test Email", outputRequest.Subject);
            Assert.Equal("<h1>Hello World</h1>", outputRequest.Body);
        }


        [Fact]
        public void TestSnailGunMapping()
        {
            var spendGridMapper = new SnailGunMapper();
            var outputRequest = spendGridMapper.MapEmailApiRequest(InputRequest) as SnailGunRequest;

            Assert.NotNull(outputRequest);
            Assert.Equal("theallanp@gmail.com", outputRequest.FromEmail);
            Assert.Equal("Allan P", outputRequest.FromName);
            Assert.Equal("susan@abcpreschool.org", outputRequest.ToEmail);
            Assert.Equal("Miss Susan", outputRequest.ToName);
            Assert.Equal("Your Daily Test Email", outputRequest.Subject);
            Assert.Equal("<h1>Hello World</h1>", outputRequest.Body);
        }
    }
}
