namespace OpenAuth.Application.Exceptions;

public class InvalidRequestException(string? description) : OAuthAuthorizationException("invalid_request", description);