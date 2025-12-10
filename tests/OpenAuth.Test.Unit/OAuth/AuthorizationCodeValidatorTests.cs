using NSubstitute;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationCodeValidatorTests
{
    private readonly ISecretQueryService _secretQueryService;
    private readonly AuthorizationCodeValidator _sut;

    private readonly AuthorizationCodeValidatorContext _validContext;
    
    public AuthorizationCodeValidatorTests()
    {
        _secretQueryService = Substitute.For<ISecretQueryService>();
        _sut = new AuthorizationCodeValidator(_secretQueryService);

        _secretQueryService
            .ValidateSecretAsync(Arg.Any<ClientId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        
        _validContext = new AuthorizationCodeValidatorContext(
            TestData.CreateValidAuthorizationCodeTokenCommand(),
            TestData.CreateValidTokenData(),
            TestData.CreateValidAuthorizationGrant());
    }

    [Fact]
    public async Task ValidateAsync_WhenPkceIsPresent_ClientSecretIsIgnored()
    {
        var ctx = _validContext with
        {
            Command = _validContext.Command with
            {
                ClientSecret = "invalid-client-secret"
            }
        };
        
        var result = await _sut.ValidateAsync(ctx);

        Assert.Equal(DefaultValues.Audience, result.AudienceName.Value);
        Assert.Equal(DefaultValues.Scopes, result.Scope.ToString());
    }
    
    [Fact]
    public async Task ValidateAsync_WhenPkceIsMissing_ClientSecretIsUsed()
    {

        var ctx = _validContext with
        {
            AuthorizationGrant = new AuthorizationGrantBuilder()
                .WithPkce(null)
                .Build()
        };
        
        var result = await _sut.ValidateAsync(ctx);

        Assert.Equal(DefaultValues.Audience, result.AudienceName.Value);
        Assert.Equal(DefaultValues.Scopes, result.Scope.ToString());
    }
    
    [Fact]
    public async Task ValidateAsync_WhenOpenIdScopes_ReturnValidationResult()
    {

        var ctx = _validContext with
        {
            AuthorizationGrant = new AuthorizationGrantBuilder()
                .WithScopes(DefaultValues.Scopes + " openid")
                .Build()
        };
        
        var result = await _sut.ValidateAsync(ctx);

        Assert.Equal(DefaultValues.Audience, result.AudienceName.Value);
        Assert.Equal(DefaultValues.Scopes, result.Scope.ToString());
        Assert.Equal("openid", result.OidcScope.ToString());
    }

    [Fact]
    public async Task ValidateAsync_WithUsedAuthorizationGrant_ThrowsInvalidGrantException()
    {
        var ctx = _validContext;
        ctx.AuthorizationGrant.Consume();
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => _sut.ValidateAsync(ctx));
    }
    
    [Fact]
    public async Task ValidateAsync_WithIncorrectClientId_ThrowsInvalidGrantException()
    {

        var ctx = _validContext with
        {
            Command = _validContext.Command with
            {
                ClientId = ClientId.New()
            }
        };
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => _sut.ValidateAsync(ctx));
    }

    [Fact]
    public async Task ValidateAsync_WithIncorrectRedirectUri_ThrowsInvalidGrantException()
    {
        var ctx = _validContext with
        {
            Command = _validContext.Command with
            {
                RedirectUri = RedirectUri.Create("https://invalid-uri.com")
            }
        };
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => _sut.ValidateAsync(ctx));       
    }
    
    [Fact]
    public async Task ValidateAsync_WithMissingCodeVerifier_ThrowsInvalidGrantException()
    {
        var ctx = _validContext with
        {
            Command = _validContext.Command with
            {
                CodeVerifier = null
            }
        };
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => _sut.ValidateAsync(ctx));
    }
    
    [Fact]
    public async Task ValidateAsync_WithIncorrectCodeVerifier_ThrowsInvalidGrantException()
    {
        var ctx = _validContext with
        {
            Command = _validContext.Command with
            {
                CodeVerifier = "invalid-code-verifier"
            }
        };
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => _sut.ValidateAsync(ctx));
    }

    [Fact]
    public async Task ValidateAsync_WithMissingClientSecret_ThrowsInvalidClientException()
    {
        // Set PKCE to null to force client secret for validation
        var ctx = _validContext with
        {
            Command = _validContext.Command with
            {
                ClientSecret = null
            },
            AuthorizationGrant = new AuthorizationGrantBuilder()
                .WithPkce(null)
                .Build()
        };

        
        await Assert.ThrowsAsync<InvalidClientException>(()
            => _sut.ValidateAsync(ctx));       
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidClientSecret_ThrowsInvalidClientException()
    {
        // Set PKCE to null to force client secret for validation
        var ctx = _validContext with
        {
            AuthorizationGrant = new AuthorizationGrantBuilder()
                .WithPkce(null)
                .Build()
        };

        _secretQueryService
            .ValidateSecretAsync(Arg.Any<ClientId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        
        await Assert.ThrowsAsync<InvalidClientException>(()
            => _sut.ValidateAsync(ctx));
    }
}