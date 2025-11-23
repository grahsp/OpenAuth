namespace OpenAuth.Application.Exceptions;

public class MalformedPkceException(string? description) : OAuthProtocolException("invalid_request", description);