namespace EmailFailOverLambda.Models
{
    public class SendGridMapper : IEmailMapper
    {
        public object MapEmailApiRequest(IEmailApiRequest emailApiRequest)
        {
            var spendGridRequest = new SpendGridRequest
            {
                Sender = $"{emailApiRequest.FromName} <{emailApiRequest.From}>",
                Recipient = $"{emailApiRequest.ToName} <{emailApiRequest.To}>",
                Subject = emailApiRequest.Subject,
                Body = emailApiRequest.Body
            };

            return spendGridRequest;
        }
    }
}
