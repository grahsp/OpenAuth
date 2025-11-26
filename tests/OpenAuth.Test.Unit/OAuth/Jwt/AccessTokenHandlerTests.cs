using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Tokens;
using OpenAuth.Application.Tokens.Builders;
using OpenAuth.Domain.OAuth;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth.Jwt;

public class AccessTokenHandlerTests
{
    private readonly IJwtSigner _signer;
    private readonly AccessTokenHandler _sut;

    private readonly AccessTokenContext _validContext;

    public AccessTokenHandlerTests()
    {
        var time = new FakeTimeProvider();
        
        var builderFactory = Substitute.For<IJwtBuilderFactory>();
        builderFactory.Create().Returns(new JwtBuilder("test-issuer", time));
        
        _signer = Substitute.For<IJwtSigner>();
        _signer.Create(Arg.Any<JwtDescriptor>(), Arg.Any<CancellationToken>())
            .Returns("access_token");
        
        _sut = new AccessTokenHandler(builderFactory, _signer);

        _validContext = TestData.CreateValidAccessTokenContext();
    }

    [Fact]
    public async Task CreateAsync_ForwardsContextToSigner()
    {
        JwtDescriptor? captured = null;
        
        _signer.Create(Arg.Do<JwtDescriptor>(ctx => captured = ctx))
            .Returns("access_token");

        await _sut.CreateAsync(_validContext);

        Assert.NotNull(captured);
        Assert.Equal(_validContext.ClientId, captured.Claims.Single(c => c.Type == OAuthClaimTypes.ClientId).Value);
        Assert.Equal(_validContext.Audience, captured.Claims.Single(c => c.Type == OAuthClaimTypes.Aud).Value);
        Assert.Equal(_validContext.Subject, captured.Claims.Single(c => c.Type == OAuthClaimTypes.Sub).Value);
        
        var scopes = captured.Claims
            .Where(c => c.Type == OAuthClaimTypes.Scope)
            .Select(c => c.Value)
            .ToArray();
        
        Assert.Equal(DefaultValues.Scopes, string.Join(' ', scopes));
    }

    [Fact]
    public async Task CreateAsync_WithoutSubject()
    {
        JwtDescriptor? captured = null;
        
        _signer.Create(Arg.Do<JwtDescriptor>(ctx => captured = ctx))
            .Returns("access_token");

        await _sut.CreateAsync(_validContext with { Subject = null });

        Assert.NotNull(captured);
        Assert.DoesNotContain(captured.Claims, c => c.Type == OAuthClaimTypes.Sub);
    }

    [Fact]
    public async Task CreateAsync_ReturnsExpectedToken()
    {
        const string tokenString = "access_token";
        _signer.Create(Arg.Any<JwtDescriptor>())
            .Returns(tokenString);

        var token = await _sut.CreateAsync(_validContext);
        
        Assert.Equal(tokenString, token);
    }
}