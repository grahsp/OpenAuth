using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Integration.Infrastructure.Clients;

public class TestClientBuilder
{
    private readonly IServiceProvider _services;
    private CreateClientRequest _request;

    public TestClientBuilder(IServiceProvider services, CreateClientRequest request)
    {
        _services = services;
        _request = request;
    }
    
    public TestClientBuilder WithRedirectUri(string redirectUri)
    {
        var redirectUris = _request.RedirectUris.ToList();
        redirectUris.Add(RedirectUri.Create(redirectUri));
        
        _request = _request with { RedirectUris = redirectUris };
        return this;
    }

    public TestClientBuilder WithPermission(string audience, string scope)
    {
        var permissions = _request.Permissions.ToList();
        permissions.Add(new Audience(AudienceName.Create(audience), ScopeCollection.Parse(scope)));
        
        _request = _request with { Permissions = permissions };
        return this;       
    }

    public async Task<RegisteredClientResponse> CreateAsync()
    {
        using var scope = _services.CreateScope();
        var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();
        
        return await clientService.RegisterAsync(_request);
    }
}