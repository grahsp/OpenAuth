namespace OpenAuth.Application.Exceptions;

public class InvalidRedirectUriException(string? description) : OAuthProtocolException("invalid_request", description);