namespace OpenAuth.Application.Exceptions;

public class InvalidGrantException(string? description) : OAuthProtocolException("invalid_grant", description);