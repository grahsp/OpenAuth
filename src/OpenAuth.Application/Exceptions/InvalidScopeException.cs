namespace OpenAuth.Application.Exceptions;

public class InvalidScopeException(string? description) : OAuthAuthorizationException("invalid_scope", description);