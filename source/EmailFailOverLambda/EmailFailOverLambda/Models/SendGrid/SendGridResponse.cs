namespace EmailFailOverLambda.Models
{
    public class SpendGridResponse
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string CreatedAt { get; set; }
    }
}
