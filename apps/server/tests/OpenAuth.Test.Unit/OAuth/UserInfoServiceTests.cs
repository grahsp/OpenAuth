using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NSubstitute;
using OpenAuth.Application.OAuth.Exceptions;
using OpenAuth.Application.OAuth.Services;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Tokens.Exceptions;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class UserInfoServiceTests
{
    private readonly IAccessTokenValidator _validator;
    private readonly IOidcUserClaimsProvider _claimsProvider;
    private readonly UserInfoService _sut;

    private static ClaimsPrincipal DefaultPrincipal => CreatePrincipal(
        "openid profile",
        DefaultValues.Subject);

    private static IReadOnlyCollection<Claim> DefaultUserClaims =>
    [
        new("sub", DefaultValues.Subject),
        new("name", "john doe"),
        new("nickname", "j-dough")
    ];

    public UserInfoServiceTests()
    {
        _validator = Substitute.For<IAccessTokenValidator>();
        _validator.ValidateAsync(Arg.Any<string>()).Returns(DefaultPrincipal);
        
        _claimsProvider = Substitute.For<IOidcUserClaimsProvider>();
        _claimsProvider.GetUserClaimsAsync(DefaultValues.Subject, Arg.Any<ScopeCollection>())
            .Returns(DefaultUserClaims);

        _sut = new UserInfoService(_validator, _claimsProvider);
    }

    private static ClaimsPrincipal CreatePrincipal(string? scope, string? subject)
    {
        var claims = new List<Claim>();

        if (scope != null)
            claims.Add(new Claim("scope", scope));

        if (subject != null)
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));

        var identity = new ClaimsIdentity(claims, "mock");
        return new ClaimsPrincipal(identity);
    } 
    

    [Fact]
    public async Task GetUserClaimsAsync_WhenValidToken_ReturnsClaimsFromProvider()
    {
        var result = await _sut.GetUserClaimsAsync("token");

        Assert.Contains(result, c => c.Type == "name");
        Assert.Contains(result, c => c.Type == "nickname");
    }

    [Fact]
    public async Task GetUserClaimsAsync_WhenValidatorThrows_Rethrows()
    {
        _validator.ValidateAsync("token").Returns<Task<ClaimsPrincipal>>(_ => 
            throw new InvalidAccessTokenException("invalid"));

        await Assert.ThrowsAsync<InvalidAccessTokenException>(() => _sut.GetUserClaimsAsync("token"));
    }

    [Fact]
    public async Task GetUserClaimsAsync_WhenScopeMissing_Throws()
    {
        var principal = CreatePrincipal(null, "123");
        _validator.ValidateAsync(Arg.Any<string>()).Returns(principal);

        await Assert.ThrowsAsync<UserInfoAccessDeniedException>(() =>
            _sut.GetUserClaimsAsync("token"));
    }

    [Fact]
    public async Task GetUserClaimsAsync_WhenScopeMissingOpenId_Throws()
    {
        var principal = CreatePrincipal("profile email", DefaultValues.Subject);
        _validator.ValidateAsync(Arg.Any<string>()).Returns(principal);

        await Assert.ThrowsAsync<UserInfoAccessDeniedException>(() =>
            _sut.GetUserClaimsAsync("token"));
    }

    [Fact]
    public async Task GetUserClaimsAsync_WhenSubjectMissing_Throws()
    {
        var principal = CreatePrincipal("openid", null);
        _validator.ValidateAsync(Arg.Any<string>()).Returns(principal);

        await Assert.ThrowsAsync<UserInfoAccessDeniedException>(() =>
            _sut.GetUserClaimsAsync("token"));
    }

    [Fact]
    public async Task GetUserClaimsAsync_CallsClaimsProviderWithCorrectValues()
    {
        await _sut.GetUserClaimsAsync("token");
    
        await _claimsProvider.Received(1).GetUserClaimsAsync(
            DefaultValues.Subject,
            ScopeCollection.Parse("openid profile")
        );
    }

    [Fact]
    public async Task GetUserClaimsAsync_WhenProviderReturnsEmpty_ReturnsEmpty()
    {
        _claimsProvider.GetUserClaimsAsync(DefaultValues.Subject, Arg.Any<ScopeCollection>())
            .Returns([]);

        var result = await _sut.GetUserClaimsAsync("token");

        Assert.Empty(result);
    }
}