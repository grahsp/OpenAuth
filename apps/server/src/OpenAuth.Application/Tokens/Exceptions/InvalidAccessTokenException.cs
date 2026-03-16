namespace OpenAuth.Application.Tokens.Exceptions;

public class InvalidAccessTokenException(string message, Exception? inner = null) : Exception(message, inner);