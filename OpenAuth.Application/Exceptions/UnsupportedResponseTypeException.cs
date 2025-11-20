namespace OpenAuth.Application.Exceptions;

public class UnsupportedResponseTypeException(string? description) : OAuthAuthorizationException("unsupported_response_type", description);