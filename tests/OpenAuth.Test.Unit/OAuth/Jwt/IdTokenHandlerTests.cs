using System.Security.Claims;
using NSubstitute;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Oidc;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.OAuth;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth.Jwt;

public class IdTokenHandlerTests
{
    private readonly IOidcUserClaimsProvider _claimsProvider;
    private readonly IJwtSigner _signer;
    private readonly IdTokenHandler _sut;

    private readonly IdTokenContext _validContext;

    public IdTokenHandlerTests()
    {
        _claimsProvider = Substitute.For<IOidcUserClaimsProvider>();
        _claimsProvider.GetUserClaimsAsync(Arg.Any<string>(), Arg.Any<ScopeCollection>())
            .Returns([
                    new Claim("username", DefaultValues.UserName),
                    new Claim("email", DefaultValues.UserEmail)
                ]);

        _signer = Substitute.For<IJwtSigner>();
        _signer.Create(Arg.Any<JwtDescriptor>(), Arg.Any<CancellationToken>())
            .Returns("id_token");
        
        _sut = new IdTokenHandler(_claimsProvider, _signer);

        _validContext = TestData.CreateValidIdTokenContext();
    }

    [Fact]
    public async Task CreateAsync_ForwardsContextToSigner()
    {
        JwtDescriptor? captured = null;
        
        _signer.Create(Arg.Do<JwtDescriptor>(ctx => captured = ctx))
            .Returns("id_token");
    
        await _sut.CreateAsync(_validContext);
    
        Assert.NotNull(captured);
        Assert.Equal(_validContext.ClientId, captured.Audience);
        Assert.Equal(_validContext.Subject, captured.Subject);
        Assert.Equal(_validContext.AuthTimeInSeconds, captured.Claims.Single(c => c.Key == "auth_time").Value);
    }

    [Fact]
    public async Task CreateAsync_WithIncorrectSubject_ThrowsInvalidClientException()
    {
        _claimsProvider.GetUserClaimsAsync(Arg.Any<string>(), Arg.Any<ScopeCollection>())
            .Returns((IEnumerable<Claim>)null!);
        
        await Assert.ThrowsAsync<InvalidClientException>(() =>
            _sut.CreateAsync(_validContext));
    }

    [Fact]
    public async Task CreateAsync_WhenNoUserClaims_OnlyIncludeRequiredClaims()
    {
        JwtDescriptor? captured = null;
        
        _signer.Create(Arg.Do<JwtDescriptor>(ctx => captured = ctx))
            .Returns("id_token");
        
        _claimsProvider.GetUserClaimsAsync(Arg.Any<string>(), Arg.Any<ScopeCollection>())
            .Returns([]);
        
        await _sut.CreateAsync(_validContext);
    
        Assert.NotNull(captured);
        Assert.DoesNotContain(captured.Claims, c => c.Key != "auth_time");
    }

    [Fact]
    public async Task CreateAsync_ReturnsExpectedToken()
    {
        const string tokenString = "id_token";
        _signer.Create(Arg.Any<JwtDescriptor>())
            .Returns(tokenString);

        var token = await _sut.CreateAsync(_validContext);
        
        Assert.Equal(tokenString, token);
    }
}