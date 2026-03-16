namespace OpenAuth.Application.Exceptions;

public class OAuthProtocolException(string error, string? description) : OAuthException(error, description);