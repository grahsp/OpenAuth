using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenAuth.Infrastructure.Persistence;
using Respawn;
using Xunit;

namespace OpenAuth.Test.Common.Infrastructure;

public class PostgresServer : IAsyncLifetime
{
    private IContainer _container = null!;
    private int _port;
    private Respawner _respawner = null!;

    public string ConnectionString =>
        $"Host=127.0.0.1;Port={_port};Database=IntegrationTests;Username=postgres;Password=postgres";

    public async Task InitializeAsync()
    {
        _container = new ContainerBuilder()
            .WithImage("postgres:16-alpine")
            .WithEnvironment("POSTGRES_DB", "IntegrationTests")
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .Build();

        await _container.StartAsync();

        _port = _container.GetMappedPublicPort(5432);

        await using var ctx = CreateContext();
        await ctx.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres
        });
    }
    
    public async Task DisposeAsync() =>
        await _container.DisposeAsync();

    private DbContextOptions<AppDbContext> CreateOptions()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString, sql =>
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
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }
}