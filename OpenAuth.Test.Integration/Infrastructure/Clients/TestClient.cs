using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;
using IAuthorizationHandler = OpenAuth.Application.OAuth.Authorization.Handlers.IAuthorizationHandler;

namespace OpenAuth.Test.Integration.Infrastructure.Clients;

public class TestClient
{
    public TestClientBuilderFactory Clients { get; }
    
    private readonly TestApplicationFixture _fx;
    
    public TestClient(TestApplicationFixture fx)
    {
        _fx = fx;
        
        var clientService = _fx.ServiceProvider.GetRequiredService<IClientService>();
        Clients = new TestClientBuilderFactory(clientService);
    }

    
    public async Task<AuthorizationGrant> AuthorizeAsync(AuthorizeCommand cmd)
    {
        var authorizationService = _fx.ServiceProvider.GetRequiredService<IAuthorizationHandler>();
        return await authorizationService.AuthorizeAsync(cmd);
    }

    public async Task<TokenGenerationResponse> RequestTokenAsync(TokenRequest request)
    {
        var tokenService = _fx.ServiceProvider.GetRequiredService<ITokenService>();
        return await tokenService.IssueToken(request);
    }
}