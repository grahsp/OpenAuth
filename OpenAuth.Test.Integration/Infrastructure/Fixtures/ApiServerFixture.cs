using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenAuth.Api;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;
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

    public async Task<ExternalOAuthClient> CreateClientAsync(ClientApplicationType type, Action<TestClientBuilder>? configure = null)
    {
        var request = new CreateClientRequest(type, ClientName.Create("test-client"), [], []);
        
        var builder = new TestClientBuilder(Services, request);
        configure?.Invoke(builder);
        
        var registered = await builder.CreateAsync();
        return new ExternalOAuthClient(_client, registered);
    }
}