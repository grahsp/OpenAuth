namespace OpenAuth.Application.OAuth.Exceptions;

public class UserInfoAccessDeniedException(string message) : Exception(message);