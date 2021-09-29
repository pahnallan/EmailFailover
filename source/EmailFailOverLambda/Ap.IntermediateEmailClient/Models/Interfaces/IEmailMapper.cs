namespace Ap.IntermediateEmailService.Models.Interfaces
{
    public interface IEmailMapper
    {
        object MapEmailApiRequest(IEmailApiRequest emailApiRequest);
    }
}
