using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.OAuth.Authorization.Flows;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationHandlerTests
{
    private readonly FakeClientQueryService _clientQueryService;
    private readonly FakeTimeProvider _time;
    private readonly IAuthorizationHandler _sut;

    private readonly Client _defaultClient;
    private readonly AuthorizationRequest _validRequest;
    
    public AuthorizationHandlerTests()
    {
        _clientQueryService = new FakeClientQueryService();
        _time = new FakeTimeProvider();
        
        _sut = new AuthorizationHandler(_clientQueryService, _time);

        _defaultClient = new ClientBuilder()
            .WithAudience("api", "read", "write")
            .WithGrantType(GrantTypes.AuthorizationCode)
            .WithRedirectUri("https://example.com/callback")
            .Build();
        _defaultClient.SetPublic(_time.GetUtcNow());
        
        _validRequest = new AuthorizationRequest(
            _defaultClient.Id,
            GrantType.AuthorizationCode,
            RedirectUri.Create("https://example.com/callback"),
            new AudienceName("api"),
            [new Scope("read"), new Scope("write")],
            "code-challenge",
            "S256"
        );
    }
    

    [Fact]
    public async Task ClientNotFound_Throws()
    {
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(_validRequest));
    }

    [Fact]
    public async Task IncorrectGrantType_Throws()
    {
        var request = _validRequest with { GrantType = GrantType.ClientCredentials };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));
    }
    
    [Fact]
    public async Task RequestContainsInvalidRedirectUri_Throws()
    {
        var request = _validRequest with { RedirectUri = RedirectUri.Create("http://invalid-uri.com/callback") };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));       
    }
    
    [Fact]
    public async Task RequestContainsInvalidAudience_Throws()
    {
        var request = _validRequest with { Audience = new AudienceName("invalid") };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));       
    }

    [Fact]
    public async Task RequestContainsInvalidScopes_Throws()
    {
        var request = _validRequest with { Scopes = [new Scope("invalid")] };
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));       
    }

    [Fact]
    public async Task MissingCodeChallenge_Throws_WhenPkceEnabled()
    {
        var request = _validRequest with { CodeChallenge = null! };
        
        // Set public to enable PCKE.
        _defaultClient.SetPublic(_time.GetUtcNow());
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));       
    }
    
    [Fact]
    public async Task MissingCodeChallengeMethod_Throws_WhenPkceEnabled()
    {
        var request = _validRequest with { CodeChallengeMethod = null! };
        
        // Set public to enable PCKE.
        _defaultClient.SetPublic(_time.GetUtcNow());
        _clientQueryService.Add(_defaultClient);
        
        await Assert.ThrowsAnyAsync<InvalidOperationException>(()
            => _sut.AuthorizeAsync(request));       
    }

    [Fact]
    public async Task IgnoreMissingCodeChallenge_WhenPkceDisabled()
    {
        var request = _validRequest with { CodeChallenge = null! };
        
        // Set to confidential and disable PCKE.
        _defaultClient.SetConfidential(_time.GetUtcNow(), false);
        _clientQueryService.Add(_defaultClient);
        
        await _sut.AuthorizeAsync(request);
    }
    
    [Fact]
    public async Task IgnoreMissingCodeChallengeMethod_WhenPkceDisabled()
    {
        var request = _validRequest with { CodeChallengeMethod = null! };
        
        // Set to confidential and disable PCKE.
        _defaultClient.SetConfidential(_time.GetUtcNow(), false);
        _clientQueryService.Add(_defaultClient);
        
        await _sut.AuthorizeAsync(request);
    }

    [Fact]
    public async Task AuthorizeAsync_ResponseContainsValidData()
    {
        _clientQueryService.Add(_defaultClient);

        var response = await _sut.AuthorizeAsync(_validRequest);
        
        Assert.Equal(_validRequest.RedirectUri, response.RedirectUri);
        Assert.NotNull(response.Code);
        Assert.NotEmpty(response.Code);
    }
}