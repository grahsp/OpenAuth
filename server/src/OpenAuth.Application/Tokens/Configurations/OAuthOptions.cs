namespace OpenAuth.Application.Tokens.Configurations;

public class OAuthOptions
{
    public const string SectionName = "OAuth";
    
    public required string Issuer { get; set; }
}