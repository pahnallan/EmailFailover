using Ap.IntermediateEmailService.Models.Interfaces;
using RestSharp;

namespace Ap.IntermediateEmailService.Services
{
    public class SnailGunService : AbstractEmailApiService
    {
        public SnailGunService(string hostUrl, string apiKey, IEmailMapper mapper)
        {
            RestClient = new RestClient(hostUrl);
            ApiKey = apiKey;
            EmailMapper = mapper;
        }
    }
}
