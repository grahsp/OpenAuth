using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.OAuth;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationHandlerTests
{
    private readonly FakeClientQueryService _clientQueryService;
    private readonly IAuthorizationGrantStore _store;
    private readonly FakeTimeProvider _time;
    private readonly IAuthorizationHandler _sut;

    private readonly Client _defaultClient;
    private readonly AuthorizeCommand _validCommand;
    
    public AuthorizationHandlerTests()
    {
        _clientQueryService = new FakeClientQueryService();
        _store = Substitute.For<IAuthorizationGrantStore>();
        _time = new FakeTimeProvider();
        
        _sut = new AuthorizationHandler(_clientQueryService, _store, _time);

        _defaultClient = new ClientBuilder()
            .WithApplicationType(ClientApplicationTypes.Spa)
            .WithAudience("api", "read", "write")
            .WithGrantType(GrantTypes.AuthorizationCode)
            .WithRedirectUri("https://example.com/callback")
            .Build();

        _validCommand = new AuthorizeCommand(
            "code",
            _defaultClient.Id.ToString(),
            "test-subject",
            "https://example.com/callback",
            "api",
            "read write",
            null,
            "code-challenge",
            "s256"
        );
    }
    

    [Fact]
    public async Task ClientNotFound_Throws()
    {
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(_validCommand));
    }
    
    [Fact]
    public async Task RequestContainsInvalidRedirectUri_Throws()
    {
        var request = _validCommand with { RedirectUri = "http://invalid-uri.com/callback" };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));
    }
    
    [Fact]
    public async Task RequestContainsInvalidAudience_Throws()
    {
        var request = _validCommand with { Audience = "invalid" };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));
    }

    [Fact]
    public async Task RequestContainsInvalidScopes_Throws()
    {
        var request = _validCommand with { Scope = "invalid" };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));
    }

    [Fact]
    public async Task WhenPublicClientAndMissingPkce_ThrowsException()
    {
        var request = _validCommand with { CodeChallenge = null };
        
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));
    }

    [Fact]
    public async Task AuthorizeAsync_ResponseContainsValidData()
    {
        _clientQueryService.Add(_defaultClient);

        var response = await _sut.AuthorizeAsync(_validCommand);
        
        Assert.Equal(_validCommand.RedirectUri, response.RedirectUri.Value);
        Assert.NotNull(response.Code);
        Assert.NotEmpty(response.Code);
    }

    [Fact]
    public async Task AddAuthorizationGrantToRepository()
    {
        _clientQueryService.Add(_defaultClient);
        
        await _sut.AuthorizeAsync(_validCommand);

        await _store
            .Received(1)
            .AddAsync(Arg.Any<AuthorizationGrant>());
    }
}