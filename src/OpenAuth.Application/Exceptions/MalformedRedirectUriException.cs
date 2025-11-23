namespace OpenAuth.Application.Exceptions;

public class MalformedRedirectUriException(string? description) : OAuthProtocolException("invalid_request", description);