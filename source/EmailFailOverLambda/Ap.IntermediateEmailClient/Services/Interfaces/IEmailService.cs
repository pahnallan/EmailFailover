using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Ap.IntermediateEmailService.Models.Interfaces;
using RestSharp;
// In the interest of time I decided to include the lambda core library in here as a dependency to access the ILambdaContext interface.
// Ideally the IntermediateEmailClient should construct and instantiate it's own logging dependencies.

namespace Ap.IntermediateEmailService.Services.Interfaces
{
    interface IEmailService
    {
        IEmailMapper EmailMapper { get; set; }
        Task<IRestResponse> SendEmailAsync(IEmailApiRequest emailApiRequest, ILambdaContext context, bool convertBodyToPlainText);
        IRestResponse SendEmail(IEmailApiRequest emailApiRequest, ILambdaContext context, bool convertBodyToPlainText);
    }
}
