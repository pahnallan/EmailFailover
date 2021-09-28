namespace EmailFailOverLambda.Models
{
    public interface IEmailApiRequest
    {
        string To { get; set; }
        string ToName { get; set; }
        string From { get; set; }
        string FromName { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
    }
}
