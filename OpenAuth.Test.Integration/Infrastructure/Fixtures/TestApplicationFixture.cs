using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Test.Integration.Infrastructure.Fixtures;

public class TestApplicationFixture : IAsyncLifetime, IDisposable
{
    public ServiceProvider ServiceProvider;
    private readonly SqlServerFixture _sql;
    
    public TestApplicationFixture(SqlServerFixture sql)
    {
        _sql = sql;
        ServiceProvider = TestCompositionRoot.BuildService(_sql.ConnectionString);
    }

    public async Task InitializeAsync() => await ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    public async Task ResetAsync()
    {
        await _sql.ResetAsync();
        ServiceProvider = TestCompositionRoot.BuildService(_sql.ConnectionString);
        
        await SeedSigningKey();
    }

    private async Task SeedSigningKey()
    {
        var signingKeyService = ServiceProvider.GetRequiredService<ISigningKeyService>();
        await signingKeyService.CreateAsync(SigningAlgorithm.RS256);
    }

    public void Dispose() => ServiceProvider.Dispose();
}