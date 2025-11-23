namespace OpenAuth.Application.Exceptions;

public class InvalidClientException(string? description) : OAuthProtocolException("invalid_client", description);