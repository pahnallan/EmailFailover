namespace EmailFailOverLambda.Models
{
    public interface IEmailMapper
    {
        object MapEmailApiRequest(IEmailApiRequest emailApiRequest);
    }
}
