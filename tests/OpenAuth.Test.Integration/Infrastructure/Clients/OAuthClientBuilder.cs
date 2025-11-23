using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Integration.Infrastructure.Clients;

public class OAuthClientBuilder
{
    private string _applicationType = DefaultValues.ApplicationType;
    private string _name = DefaultValues.ClientName;
    private readonly List<string> _redirectUris = [DefaultValues.RedirectUri];
    private readonly Dictionary<string, string> _permissions = new()
    {
        { DefaultValues.Audience, DefaultValues.Scopes }
    };
    
    private readonly IServiceProvider _services;

    public OAuthClientBuilder(IServiceProvider services)
    {
        _services = services;
    }

    public OAuthClientBuilder WithApplicationType(string applicationType)
    {
        _applicationType = applicationType;
        return this;
    }

    public OAuthClientBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public OAuthClientBuilder WithRedirectUri(string redirectUri)
    {
        _redirectUris.Add(redirectUri);
        return this;
    }

    public OAuthClientBuilder WithPermission(string audience, string scopes)
    {
        _permissions.TryAdd(audience, scopes);
        return this;       
    }

    public async Task<RegisteredClientResponse> CreateAsync()
    {
        using var scope = _services.CreateScope();
        var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

        var applicationType = ClientApplicationTypes.Parse(_applicationType);
        var name = ClientName.Create(_name);
        var redirectUris = _redirectUris.Select(x => new RedirectUri(x)).ToList();
        var permissions = _permissions.Select(kvp =>
        {
            var audience = AudienceName.Create(kvp.Key);
            var scopes = ScopeCollection.Parse(kvp.Value);

            return new Audience(audience, scopes);
        });

        var request = new CreateClientRequest(applicationType, name, permissions, redirectUris);
        return await clientService.RegisterAsync(request);
    }
}