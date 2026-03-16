namespace OpenAuth.Application.Shared;

public class NotFoundException(string message) : Exception(message);