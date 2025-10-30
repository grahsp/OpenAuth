namespace OpenAuth.Domain.Clients.ValueObjects;

public readonly record struct RedirectUri(string Value)
{
    public static RedirectUri Create(string uri)
    {
        if (!TryCreate(uri, out var redirectUri))
            throw new ArgumentException($"Invalid redirect URI: {uri}", nameof(uri));

        return redirectUri;
    }

    public static bool TryCreate(string uri, out RedirectUri redirectUri)
    {
        redirectUri = default;
        
        if (string.IsNullOrWhiteSpace(uri))
            return false;

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            return false;

        if (parsed.Scheme is not ("http" or "https"))
            return false;
        
        redirectUri = new RedirectUri(parsed.AbsoluteUri);
        return true;
    }
    
    public override string ToString() => Value;
}