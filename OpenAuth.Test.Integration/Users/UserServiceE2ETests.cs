using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OpenAuth.Api;
using OpenAuth.Api.Controllers;
using OpenAuth.Test.Integration.Infrastructure;

namespace OpenAuth.Test.Integration.Users;

[Collection("sqlserver")]
public class UserServiceE2ETests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    private SqlServerFixture _fx;
    private TimeProvider _time;
    
    public UserServiceE2ETests(SqlServerFixture fx)
    {
        _fx = fx;
        _time = TimeProvider.System;

        _factory = new TestWebApplicationFactory(_fx.ConnectionString);
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync()
        => _fx.ResetAsync();

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private (string username, string email, string password) CreateValidRequest()
        => ("John", "john@example.com", "password");


    [Fact]
    public async Task RegisterUserAsync_PersistsUserToDatabase()
    {
        var now = _time.GetUtcNow();
        var (username, email, password) = CreateValidRequest();

        var request = new RegisterUserRequest(username, email, password);
        var response = await _client.PostAsJsonAsync("/api/users", request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();

        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.Equal(email, result.Email);
        Assert.InRange(result.CreatedAt, now, now.AddSeconds(5));
    }
}