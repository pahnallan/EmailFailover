namespace EmailFailOverLambda.Models
{
    public class SnailGunMapper : IEmailMapper
    {
        public object MapEmailApiRequest(IEmailApiRequest emailApiRequest)
        {
            var snailGunRequest = new SnailGunRequest
            {
                FromEmail = emailApiRequest.From,
                FromName = emailApiRequest.FromName,
                ToEmail = emailApiRequest.To,
                ToName = emailApiRequest.ToName,
                Subject = emailApiRequest.Subject,
                Body = emailApiRequest.Body
            };

            return snailGunRequest;
        }
    }
}
