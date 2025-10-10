namespace OpenAuth.Domain.Clients.ValueObjects;

public record RedirectUri
{
    public string Value { get; private init; }
    
    private RedirectUri() { }

    public static RedirectUri Create(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Redirect URI cannot be empty.", nameof(uri));

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            throw new ArgumentException($"Invalid URI format: { parsed }.", nameof(uri));
        
        if (parsed.Scheme is not ("http" or "https"))
            throw new ArgumentException("Only HTTP and HTTPS schemes are supported.", nameof(uri));
        
        return new RedirectUri { Value = uri };
    }
}