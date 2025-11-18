using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Integration.Infrastructure.Clients;

public class TestClientBuilder
{
    private readonly IClientService _service;
    private CreateClientRequest _request;

    public TestClientBuilder(IClientService service, CreateClientRequest request)
    {
        _service = service;
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
        => await _service.RegisterAsync(_request);
}