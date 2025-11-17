using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Test.Integration.Infrastructure;
using IAuthorizationHandler = OpenAuth.Application.OAuth.Authorization.Handlers.IAuthorizationHandler;

namespace OpenAuth.Test.Integration.OAuth;

public class TestOAuthClient(TestApplication fx)
{
    public async Task<RegisteredClientResponse> SetupSpaClientAsync(CreateClientRequest? request = null)
    {
        request ??= new CreateClientRequest(
            ClientApplicationTypes.M2M,
            ClientName.Create("test client"),
            [],
            [RedirectUri.Create("https://example.com/callback")]
        );
        
        var clientResult = await SetupClientAsync(request);
        return clientResult;
    }

    public async Task<RegisteredClientResponse> SetupClientAsync(CreateClientRequest request)
    {
        var clientService = fx.ServiceProvider.GetRequiredService<IClientService>();
        
        await CreateSigningKeyAsync(SigningAlgorithm.RS256);
        return await clientService.RegisterAsync(request);
    }
    
    public async Task<SigningKeyInfo> CreateSigningKeyAsync(SigningAlgorithm algorithm)
    {
        var signingKeyService = fx.ServiceProvider.GetRequiredService<ISigningKeyService>();
        return await signingKeyService.CreateAsync(algorithm);
    }
    
    public async Task<AuthorizationGrant> AuthorizeAsync(AuthorizeCommand cmd)
    {
        var authorizationService = fx.ServiceProvider.GetRequiredService<IAuthorizationHandler>();
        return await authorizationService.AuthorizeAsync(cmd);
    }

    public async Task<TokenGenerationResponse> RequestTokenAsync(TokenRequest request)
    {
        var tokenService = fx.ServiceProvider.GetRequiredService<ITokenService>();
        return await tokenService.IssueToken(request);
    }
}