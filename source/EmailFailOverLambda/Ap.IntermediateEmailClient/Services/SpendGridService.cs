using Ap.IntermediateEmailService.Models.Interfaces;
using RestSharp;

namespace Ap.IntermediateEmailService.Services
{
    public class SpendGridService : AbstractEmailApiService
    {
        public SpendGridService(string hostUrl, string apiKey, IEmailMapper mapper)
        {
            RestClient = new RestClient(hostUrl);
            ApiKey = apiKey;
            EmailMapper = mapper;
        }
    }
}
