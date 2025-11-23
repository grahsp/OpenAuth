namespace OpenAuth.Domain.Shared;

internal static class DomainErrors
{
    public static class OAuth
    {
        public const string AudienceRequired = "Client must have at least one audience.";
        public const string GrantTypeRequired = "Client must have at least one grant type.";
    }
}