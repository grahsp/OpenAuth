namespace OpenAuth.Application.Exceptions;

public class OAuthAuthorizationException(string error, string? description) : OAuthException(error, description);