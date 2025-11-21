using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Test.Integration.Infrastructure.Clients;

namespace OpenAuth.Test.Integration.Infrastructure.Fixtures;

public class ApplicationFixture : IAsyncLifetime, IDisposable
{
    private readonly SqlServerFixture _sql;
    private ServiceProvider _services;
    
    public ApplicationFixture(SqlServerFixture sql)
    {
        _sql = sql;
        _services = CompositionRoot.BuildService(_sql.ConnectionString);
    }

    public async Task InitializeAsync() => await ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    public async Task ResetAsync()
    {
        await _sql.ResetAsync();
        _services = CompositionRoot.BuildService(_sql.ConnectionString);
        
        await SeedSigningKey();
    }

    private async Task SeedSigningKey()
    {
        var signingKeyService = _services.GetRequiredService<ISigningKeyService>();
        await signingKeyService.CreateAsync(SigningAlgorithm.RS256);
    }

    public void Dispose() => _services.Dispose();
    
    
    public async Task<InternalOAuthClient> CreateClientAsync(ClientApplicationType type, Action<TestClientBuilder>? configure = null)
    {
        var request = new CreateClientRequest(
            type,
            ClientName.Create("test-client"),
            [new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"))],
            [RedirectUri.Create("https://example.com")]
        );
        
        var builder = new TestClientBuilder(_services, request);
        configure?.Invoke(builder);

        var registered = await builder.CreateAsync();

        var scope = _services.CreateScope();
        var services = scope.ServiceProvider;
        
        var authorizationHandler = services.GetRequiredService<IAuthorizationHandler>();
        var tokenService = services.GetRequiredService<ITokenService>();
        
        return new InternalOAuthClient(authorizationHandler, tokenService, registered);
    }
}