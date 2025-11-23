namespace OpenAuth.Application.Exceptions;

public class MalformedClientException(string? description) : OAuthProtocolException("invalid_request", description);