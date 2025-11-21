using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenAuth.Api;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Test.Integration.Infrastructure.Clients;
using OpenAuth.Test.Integration.Infrastructure.Fakes;

namespace OpenAuth.Test.Integration.Infrastructure.Fixtures;

public class ApiServerFixture(SqlServerFixture sql) : WebApplicationFactory<Program>, IAsyncLifetime
{
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        
        await sql.ResetAsync();
        await SeedSigningKeyAsync();
    }

    public new Task DisposeAsync() => Task.CompletedTask;


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(sql.ConnectionString));

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>("Test", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });
        });
    }

    private async Task SeedSigningKeyAsync()
    {
        var scope = Services.CreateScope();
        var signingService = scope.ServiceProvider.GetRequiredService<ISigningKeyService>();

        await signingService.CreateAsync(SigningAlgorithm.RS256);
    }

    public async Task<ExternalOAuthClient> CreateClientAsync(Action<OAuthClientBuilder>? configure = null)
    {
        var builder = new OAuthClientBuilder(Services);
        configure?.Invoke(builder);
        
        var registered = await builder.CreateAsync();
        return new ExternalOAuthClient(_client, registered);
    }
}