using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class OAuthClientBuilder
{
    private string _applicationType = DefaultValues.ApplicationType;
    private string _name = DefaultValues.ClientName;
    private readonly List<string> _redirectUris = [DefaultValues.RedirectUri];
    
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

    public async Task<RegisteredClientResponse> CreateAsync()
    {
        var clientService = _services.GetRequiredService<IClientService>();

        var applicationType = ClientApplicationTypes.Parse(_applicationType);
        var name = new ClientName(_name);
        var redirectUris = _redirectUris.Select(x => new RedirectUri(x)).ToList();

        var request = new CreateClientRequest(applicationType, name, redirectUris);
        return await clientService.RegisterAsync(request);
    }
}