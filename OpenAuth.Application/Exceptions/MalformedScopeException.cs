namespace OpenAuth.Application.Exceptions;

public class MalformedScopeException(string? description) : OAuthProtocolException("invalid_request", description);