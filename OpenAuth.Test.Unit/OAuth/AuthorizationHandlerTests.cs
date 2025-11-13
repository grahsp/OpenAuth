using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.OAuth.Authorization.Dtos;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.AuthorizationGrants.Enums;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;
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
    private readonly AuthorizationRequest _validRequest;
    
    public AuthorizationHandlerTests()
    {
        _clientQueryService = new FakeClientQueryService();
        _store = Substitute.For<IAuthorizationGrantStore>();
        _time = new FakeTimeProvider();
        
        _sut = new AuthorizationHandler(_clientQueryService, _store, _time);

        _defaultClient = new ClientBuilder()
            .WithAudience("api", "read", "write")
            .WithGrantType(GrantTypes.AuthorizationCode)
            .WithRedirectUri("https://example.com/callback")
            .Build();

        _validRequest = new AuthorizationRequest(
            _defaultClient.Id,
            RedirectUri.Create("https://example.com/callback"),
            new AudienceName("api"),
            ScopeCollection.Parse("read write"), 
            Pkce.Create("code-challenge", CodeChallengeMethod.S256)
        );
    }
    

    [Fact]
    public async Task ClientNotFound_Throws()
    {
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(_validRequest, "subject"));
    }
    
    [Fact]
    public async Task RequestContainsInvalidRedirectUri_Throws()
    {
        var request = _validRequest with { RedirectUri = RedirectUri.Create("http://invalid-uri.com/callback") };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request, "subject"));
    }
    
    [Fact]
    public async Task RequestContainsInvalidAudience_Throws()
    {
        var request = _validRequest with { Audience = new AudienceName("invalid") };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request, "subject"));
    }

    [Fact]
    public async Task RequestContainsInvalidScopes_Throws()
    {
        var request = _validRequest with { Scopes = ScopeCollection.Parse("invalid") };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request, "subject"));
    }

    [Fact]
    public async Task MissingPkce_Throws_WhenPkceEnabled()
    {
        var request = _validRequest with { Pkce = null! };
        
        _defaultClient.SetPkceRequirement(true, _time.GetUtcNow());
        
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request, "subject"));
    }

    [Fact]
    public async Task IgnoreMissingPkce_WhenPkceDisabled()
    {
        var request = _validRequest with { Pkce = null! };
        var now = _time.GetUtcNow();
        
        _defaultClient.SetPkceRequirement(false, now);
        
        _clientQueryService.Add(_defaultClient);
        
        await _sut.AuthorizeAsync(request, "subject");
    }

    [Fact]
    public async Task AuthorizeAsync_ResponseContainsValidData()
    {
        _clientQueryService.Add(_defaultClient);

        var response = await _sut.AuthorizeAsync(_validRequest, "subject");
        
        Assert.Equal(_validRequest.RedirectUri, response.RedirectUri);
        Assert.NotNull(response.Code);
        Assert.NotEmpty(response.Code);
    }

    [Fact]
    public async Task AddAuthorizationGrantToRepository()
    {
        _clientQueryService.Add(_defaultClient);
        
        await _sut.AuthorizeAsync(_validRequest, "subject");

        await _store
            .Received(1)
            .AddAsync(Arg.Any<AuthorizationGrant>());
    }
}