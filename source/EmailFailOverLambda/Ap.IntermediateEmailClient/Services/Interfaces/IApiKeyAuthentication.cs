namespace Ap.IntermediateEmailService.Services.Interfaces
{
    interface IApiKeyAuthentication
    {
        string SecretManageApiKeyName { get; set; }

        string ApiKey { get; set; }
    }
}
