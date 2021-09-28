namespace EmailFailOverLambda.Service
{
    interface IApiKeyAuthentication
    {
        string SecretManageApiKeyName { get; set; }

        string ApiKey { get; set; }
    }
}
