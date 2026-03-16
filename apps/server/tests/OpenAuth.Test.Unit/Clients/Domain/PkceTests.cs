using OpenAuth.Domain.AuthorizationGrants.Enums;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain;

public class PkceTests
{
    private const string DefaultCodeChallenge = "code";

    private static Pkce CreateDefaultPkce(CodeChallengeMethod method)
        => Pkce.Create(DefaultCodeChallenge, method);
    
    
    [Theory]
    [InlineData(CodeChallengeMethod.Plain)]
    [InlineData(CodeChallengeMethod.S256)]
    public void Create_WithValidInput_CreatesExpectedPkce(CodeChallengeMethod method)
    {
        var pkce = CreateDefaultPkce(method);

        Assert.Equal(DefaultCodeChallenge, pkce.CodeChallenge);
        Assert.Equal(method, pkce.CodeChallengeMethod);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCodeChallenge_ThrowsArgumentException(string? codeChallenge)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Pkce.Create(codeChallenge!, CodeChallengeMethod.Plain));
    }
    

    [Theory]
    [InlineData(CodeChallengeMethod.Plain)]
    [InlineData(CodeChallengeMethod.S256)]
    public void Matches_WithValidInput_ReturnsTrue(CodeChallengeMethod method)
    {
        var codeChallenge = Pkce.ComputeChallenge(DefaultCodeChallenge, method);
        var pkce = Pkce.Create(codeChallenge, method);
        
        Assert.True(pkce.Matches(DefaultCodeChallenge));
    }
    
    [Fact]
    public void Matches_WithIncorrectMethod_ReturnsFalse()
    {
        var pkce = Pkce.Create(DefaultCodeChallenge, CodeChallengeMethod.S256);
        Assert.False(pkce.Matches(DefaultCodeChallenge));
    }

    [Fact]
    public void Matches_WithDifferentCasingCodeChallenge_ReturnsFalse()
    {
        var pkce = Pkce.Create(DefaultCodeChallenge.ToUpper(), CodeChallengeMethod.Plain);
        Assert.False(pkce.Matches(DefaultCodeChallenge));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("incorrect-code-verifier")]
    public void Matches_WithIncorrectCodeVerifier_ReturnsFalse(string? codeVerifier)
    {
        var pkce = CreateDefaultPkce(CodeChallengeMethod.Plain);
        Assert.False(pkce.Matches(codeVerifier));
    }
}