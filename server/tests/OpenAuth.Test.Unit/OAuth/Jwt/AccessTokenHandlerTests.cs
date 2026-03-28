using NSubstitute;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Tokens;
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
        _signer = Substitute.For<IJwtSigner>();
        _signer.Create(Arg.Any<JwtDescriptor>(), Arg.Any<CancellationToken>())
            .Returns("access_token");
        
        _sut = new AccessTokenHandler(_signer);

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
        Assert.Equal(_validContext.Audience, captured.Audience);
        Assert.Equal(_validContext.Subject, captured.Subject);
        Assert.Equal(_validContext.Scope.ToString(), captured.Claims.Single(c => c.Key == "scope").Value);
    }

    [Fact]
    public async Task CreateAsync_WithoutSubject()
    {
        JwtDescriptor? captured = null;
        
        _signer.Create(Arg.Do<JwtDescriptor>(ctx => captured = ctx))
            .Returns("access_token");
    
        await _sut.CreateAsync(_validContext with { Subject = null });
    
        Assert.NotNull(captured);
        Assert.Null(captured.Subject);
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