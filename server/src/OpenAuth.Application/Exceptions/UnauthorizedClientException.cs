namespace OpenAuth.Application.Exceptions;

public class UnauthorizedClientException(string? description) : OAuthProtocolException("unauthorized_client", description);