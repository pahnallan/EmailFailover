using System.Threading.Tasks;
using EmailFailOverLambda.Models;
using RestSharp;

namespace EmailFailOverLambda.Service
{
    interface IEmailService
    {
        IEmailMapper EmailMapper { get; set; }
        Task<IRestResponse> SendEmailAsync(IEmailApiRequest emailApiRequest);
    }
}
