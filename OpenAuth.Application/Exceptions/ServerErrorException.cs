namespace OpenAuth.Application.Exceptions;

public class ServerErrorException(string message) : Exception(message);