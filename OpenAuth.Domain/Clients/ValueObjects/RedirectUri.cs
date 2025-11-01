using System.Diagnostics.CodeAnalysis;
using OpenAuth.Domain.Shared.Interfaces;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record RedirectUri(string Value) : ICreate<string, RedirectUri>
{
    public static RedirectUri Create(string uri)
    {
        if (!TryCreate(uri, out var redirectUri))
            throw new ArgumentException($"Invalid redirect URI: {uri}", nameof(uri));

        return redirectUri;
    }

    public static bool TryCreate(string uri, [NotNullWhen(true)] out RedirectUri? redirectUri)
    {
        redirectUri = null;
        
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