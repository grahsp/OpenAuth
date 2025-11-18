using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Infrastructure.Persistence;
using Respawn;

namespace OpenAuth.Test.Integration.Infrastructure.Fixtures;

public class SqlServerFixture : IAsyncLifetime
{
    private IContainer _container = null!;
    private int _port;
    private Respawner _respawner = null!;

    public string ConnectionString =>
        $"Server=127.0.0.1,{_port};Database=IntegrationTest;User Id=sa;Password=Passw0rd!123;TrustServerCertificate=True;Encrypt=False;";

    public async Task InitializeAsync()
    {
        _container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_SA_PASSWORD", "Passw0rd!123")
            .WithPortBinding(1433, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(1433))
            .Build();

        await _container.StartAsync();
        _port = _container.GetMappedPublicPort(1433);

        await using var ctx = CreateContext();
        await ctx.Database.EnsureCreatedAsync();

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"]
        });
    }
    
    public async Task DisposeAsync() =>
        await _container.DisposeAsync();

    private DbContextOptions<AppDbContext> CreateOptions()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString, sql =>
            {
                sql.EnableRetryOnFailure();
                sql.CommandTimeout(60);
            });

        return builder.Options;
    }

    public AppDbContext CreateContext(DbContextOptions<AppDbContext>? options = null)
    {
        var opts = options ?? CreateOptions();
        return new AppDbContext(opts);
    }

    public async Task ResetAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }
}