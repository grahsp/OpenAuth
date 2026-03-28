namespace OpenAuth.Application.Exceptions;

public class OAuthException : Exception
{
    public string Error { get; }
    public string? Description { get; }
    
    public OAuthException(string error, string? description = null)
    {
        Error = error;
        Description = description;
    }
}