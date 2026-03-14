using NSubstitute;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationCodeValidatorTests
{
    private sealed class TestContext
    {
        public ApiResourceBuilder Api { get; } = new ApiResourceBuilder();
        public AuthorizationGrantBuilder AuthorizationGrant { get; } = new AuthorizationGrantBuilder();
        public AuthorizationCodeTokenCommandBuilder Command { get; } = new AuthorizationCodeTokenCommandBuilder();
        public ClientTokenData TokenData { get; } = TestData.CreateValidTokenData();
        
        public ISecretQueryService SecretQueryService { get; }
        public AuthorizationCodeValidator Sut { get; }
        
        public TestContext()
        {
            SecretQueryService = Substitute.For<ISecretQueryService>();

            WithSecret();

            Command.WithCodeVerifier(DefaultValues.CodeVerifier);
            
            Sut = new AuthorizationCodeValidator(SecretQueryService);
        }
        
        public async Task<AuthorizationCodeValidationResult> ValidateAsync(AuthorizationGrant? grant = null) =>
            await Sut.ValidateAsync(Command.Build(), TokenData, grant ?? AuthorizationGrant.Build(), Api.Build());

        public void WithSecret(bool exists = true)
        {
            SecretQueryService
                .ValidateSecretAsync(Arg.Any<ClientId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(exists);
        }
    }
    
    [Fact]
    public async Task ValidateAsync_WhenPkceIsPresent_ClientSecretIsIgnored()
    {
        var context = new TestContext();
        context.Command.WithClientSecret("invalid-client-secret");
        
        var result = await context.ValidateAsync();
        Assert.Equal(DefaultValues.Scopes, result.Scope.ToString());
    }
    
    [Fact]
    public async Task ValidateAsync_WhenPkceIsMissing_ClientSecretIsUsed()
    {
        var context = new TestContext();
        context.Command
            .WithCodeVerifier(null)
            .WithClientSecret(DefaultValues.ClientSecret);
        
        context.AuthorizationGrant.WithPkce(null);
        
        var result = await context.ValidateAsync();
        Assert.Equal(DefaultValues.Scopes, result.Scope.ToString());
    }

    [Fact]
    public async Task ValidateAsync_WhenAudienceMismatch()
    {
        var context = new TestContext();
        context.AuthorizationGrant.WithAudience("invalid-audience");
        
        await Assert.ThrowsAsync<InvalidGrantException>(() => context.ValidateAsync());
    }

    [Fact]
    public async Task ValidateAsync_WhenScopeHasNotBeenGranted_Throws()
    {
        var context = new TestContext();
        
        context.AuthorizationGrant.WithScopes("read write");
        context.Command.WithScopes("read write invalid-scope");
        
        await Assert.ThrowsAsync<InvalidScopeException>(() => context.ValidateAsync());
    }

    [Fact]
    public async Task ValidateAsync_WhenScopeIsMissing_ReturnsAllGrantedScopes()
    {
        var context = new TestContext();
        
        context.Command.WithScopes(null);
        var grant = context.AuthorizationGrant
            .WithScopes("read write")
            .Build();
        
        var result = await context.ValidateAsync();
        Assert.Equal(grant.GrantedScopes, result.Scope);
    }
    
    [Fact]
    public async Task ValidateAsync_WhenOpenIdScopes_ReturnValidationResult()
    {
        var context = new TestContext();
        
        const string expected = "openid profile";
        context.Command.WithScopes(expected);
        context.AuthorizationGrant.WithScopes(expected);

        var result = await context.ValidateAsync();
        Assert.Contains(expected, result.OidcScope.ToString());
    }

    [Fact]
    public async Task ValidateAsync_WithUsedAuthorizationGrant_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        var grant = context.AuthorizationGrant.Build();
        
        grant.Consume();
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => context.ValidateAsync(grant: grant));
    }
    
    [Fact]
    public async Task ValidateAsync_WithIncorrectClientId_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        context.Command.WithClientId(ClientId.New());
        
        await Assert.ThrowsAsync<InvalidGrantException>(() => context.ValidateAsync());
    }

    [Fact]
    public async Task ValidateAsync_WithIncorrectRedirectUri_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        context.Command.WithRedirectUri("https://invalid-uri.com");
        
        await Assert.ThrowsAsync<InvalidGrantException>(() => context.ValidateAsync());
    }
    
    [Fact]
    public async Task ValidateAsync_WithMissingCodeVerifier_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        context.Command
            .WithCodeVerifier(null)
            .WithClientSecret("plain-client-secret");
        
        await Assert.ThrowsAsync<InvalidGrantException>(() => context.ValidateAsync());
    }
    
    [Fact]
    public async Task ValidateAsync_WithIncorrectCodeVerifier_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        context.Command.WithCodeVerifier("invalid-code-verifier");
        
        await Assert.ThrowsAsync<InvalidGrantException>(() => context.ValidateAsync());
    }

    [Fact]
    public async Task ValidateAsync_WithMissingClientSecret_ThrowsInvalidClientException()
    {
        // Set PKCE to null to force client secret for validation
        var context = new TestContext();
        
        context.Command.WithClientSecret(null);
        context.AuthorizationGrant.WithPkce(null);
        
        await Assert.ThrowsAsync<InvalidClientException>(() => context.ValidateAsync());
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidClientSecret_ThrowsInvalidClientException()
    {
        // Set PKCE to null to force client secret for validation
        var context = new TestContext();
        
        context.AuthorizationGrant.WithPkce(null);
        context.WithSecret(false);
        
        await Assert.ThrowsAsync<InvalidClientException>(() => context.ValidateAsync());
    }
}