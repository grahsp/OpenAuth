using Microsoft.AspNetCore.WebUtilities;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Test.Common.ValueObjects;
using OpenAuth.Test.Integration.Infrastructure.Builders;

namespace OpenAuth.Test.Integration.Infrastructure.Clients;

public class ExternalOAuthClient
{
    public string ApplicationType { get; }
    public string Id { get; }
    public IReadOnlyCollection<string> RedirectUris { get; }
    
    public string? Secret { get; }
    
    private readonly HttpClient _httpClient;

    
    public ExternalOAuthClient(HttpClient httpClient, RegisteredClientResponse registered)
    {
        _httpClient = httpClient;

        var client = registered.Client;
        Id = client.Id.ToString();
        ApplicationType = client.ApplicationType.Name;
        RedirectUris = client.RedirectUris
            .Select(uri => uri.ToString())
            .ToArray();
        
        Secret = registered.ClientSecret;
    }

    public async Task<AuthorizeResult> AuthorizeAsync(Action<AuthorizeUriBuilder> queryParameters)
    {
        var builder = new AuthorizeUriBuilder();
        queryParameters.Invoke(builder);

        var uri = builder.Build();
        
        var response = await _httpClient.GetAsync(uri);
        
        var redirect = response.Headers.Location;
        if (redirect is null)
            return new AuthorizeResult(false, null, null, null);
        
        var query = QueryHelpers.ParseQuery(redirect.Query);
        var result = new AuthorizeResult(
            Success: !query.ContainsKey("error"),
            Code: query.TryGetValue("code", out var code) ? code.ToString() : null,
            State: query.TryGetValue("state", out var s) ? s.ToString() : null,
            RedirectUri: redirect?.ToString(),
            Error: query.TryGetValue("error", out var e) ? e.ToString() : null);

        return result;
    }
}