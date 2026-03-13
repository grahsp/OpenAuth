namespace OpenAuth.Application.Exceptions;

public class InvalidAudienceException(string? description) : OAuthAuthorizationException("invalid_request", description);