using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Test.Integration.Infrastructure;

namespace OpenAuth.Test.Integration.Clients.Application;


[Collection("sqlserver")]
public class ClientServiceTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly ClientService _sut;

    public ClientServiceTests(SqlServerFixture fx)
    {
        _fx = fx;
        var time = TimeProvider.System;
        
        var context = _fx.CreateContext();
        var repo = new ClientRepository(context);
        var factory = new ClientFactory(time);

        _sut = new ClientService(repo, factory, time);
    }
    
    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    
    [Fact]
    public async Task RegisterAsync_PersistsNewClient()
    {
        var created = await _sut.RegisterAsync(new ClientName("client"));
        
        await using var ctx = _fx.CreateContext();
        var fetched = await ctx.Clients.SingleAsync(c => c.Id == created.Id);

        Assert.NotNull(fetched);
        Assert.Equal(created.Name, fetched.Name);
        Assert.Equal(created.Id, fetched.Id);
    }
    
    [Fact]
    public async Task RenameAsync_PersistsUpdatedName()
    {
        var initialName = new ClientName("new-client");
        var expectedName = new ClientName("old-client");
            
        var client = await _sut.RegisterAsync(initialName);
        await _sut.RenameAsync(client.Id, expectedName);

        await using var ctx = _fx.CreateContext();
        var fetched = await ctx.Clients.SingleAsync(c => c.Id == client.Id);
        
        Assert.NotNull(fetched);
        Assert.Equal(expectedName, fetched.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesClient()
    {
        var clientInfo = await _sut.RegisterAsync(new ClientName("client"));
        await _sut.DeleteAsync(clientInfo.Id);

        await using var ctx = _fx.CreateContext();
        Assert.False(await ctx.Clients.AnyAsync());
    }
    
    [Fact]
    public async Task EnableAsync_PersistsEnabledFlag()
    {
        var clientInfo = await _sut.RegisterAsync(new ClientName("client"));
        await _sut.DisableAsync(clientInfo.Id);
        await _sut.EnableAsync(clientInfo.Id);
        
        await using var ctx = _fx.CreateContext();
        var fetched = await ctx.Clients.SingleAsync(c => c.Id == clientInfo.Id);
        
        Assert.True(fetched.Enabled);
    }

    [Fact]
    public async Task DisableAsync_PersistsEnabledFlag()
    {
        var clientInfo = await _sut.RegisterAsync(new ClientName("client"));
        await _sut.DisableAsync(clientInfo.Id);
        
        await using var ctx = _fx.CreateContext();
        var fetched = await ctx.Clients.SingleAsync(c => c.Id == clientInfo.Id);
        
        Assert.False(fetched.Enabled);
    }
}