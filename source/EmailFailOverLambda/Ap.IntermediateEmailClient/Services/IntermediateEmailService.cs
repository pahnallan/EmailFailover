using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Ap.IntermediateEmailClient.Models.Enums;
using Ap.IntermediateEmailService.Models.Interfaces;
using Ap.IntermediateEmailService.Models.IntermediateEmailClient;
using Ap.IntermediateEmailService.Models.SnailGun;
using Ap.IntermediateEmailService.Models.SpendGrid;
using Ap.IntermediateEmailService.Services.Interfaces;
using Newtonsoft.Json;
using ReadSharp;
using RestSharp;

namespace Ap.IntermediateEmailService.Services
{
    public class IntermediateEmailService : AbstractEmailApiService
    {
        private readonly SpendGridService _spendGridClient;
        private readonly SnailGunService _snailGunService;

        private  EmailProviders CurrentActiveEmailProvider { get; set; }

        public IntermediateEmailService(string spendGridUrl, string spendGridApiKey, string snailGunUrl, string snailGunApiKey, string activeEmailProvider)
        {
            _spendGridClient = new SpendGridService(spendGridUrl, spendGridApiKey, new SendGridMapper());
            _snailGunService = new SnailGunService(snailGunUrl, snailGunApiKey, new SnailGunMapper());

            UpdateCurrentActiveEmailProvider(activeEmailProvider);
        }



        public override async Task<IRestResponse> SendEmailAsync(IEmailApiRequest emailApiRequest, ILambdaContext context, bool convertBodyToPlainText = true)
        {
            var emailClient = GetActiveEmailClient();
            var response = await emailClient.SendEmailAsync(emailApiRequest, context, convertBodyToPlainText);

            return response;
        }

        public void UpdateCurrentActiveEmailProvider(string emailProvider)
        {
            Enum.TryParse(emailProvider, out EmailProviders activeEmailProvider);
            if (emailProvider == null)
            {
                throw new NotImplementedException($"Email Provider {emailProvider} is not implemented.");
            }

            CurrentActiveEmailProvider = activeEmailProvider;
        }



        private IEmailService GetEmailClient(string emailProvider)
        {
            Enum.TryParse(emailProvider, out EmailProviders activeEmailProvider);
            switch (activeEmailProvider)
            {
                case EmailProviders.SpendGrid:
                    return _spendGridClient;
                case EmailProviders.SnailGun:
                    return _snailGunService;
                default:
                    throw new NotImplementedException($"Email Provider {emailProvider} is not implemented.");
            }
        }

        private IEmailService GetActiveEmailClient()
        {
            switch (CurrentActiveEmailProvider)
            {
                case EmailProviders.SpendGrid:
                    return _spendGridClient;
                case EmailProviders.SnailGun:
                    return _snailGunService;
                default:
                    throw new NotImplementedException($"Email Provider {CurrentActiveEmailProvider} is not implemented.");
            }
        }

        public static List<string> ValidateEmailApiRequest(IEmailApiRequest emailApiRequest)
        {
            var validationErrors = new List<string>();

            if (!string.IsNullOrWhiteSpace(emailApiRequest.From) || !IsValidEmail(emailApiRequest.From))
            {
                validationErrors.Add("Please enter a valid email address in the From request field.");
            }

            if (!string.IsNullOrWhiteSpace(emailApiRequest.To) || !IsValidEmail(emailApiRequest.To))
            {
                validationErrors.Add("Please enter a valid email address in the To request field.");
            }

            if (!string.IsNullOrWhiteSpace(emailApiRequest.Subject) )
            {
                validationErrors.Add("Subject field may not be empty.");
            }

            if (!string.IsNullOrWhiteSpace(emailApiRequest.Body))
            {
                validationErrors.Add("Body field may not be empty.");
            }

            return validationErrors;
        }

        private static bool IsValidEmail(string emailAddress)
        {
            EmailAddressAttribute e = new EmailAddressAttribute();
            return e.IsValid(emailAddress);
        }
    }
}
