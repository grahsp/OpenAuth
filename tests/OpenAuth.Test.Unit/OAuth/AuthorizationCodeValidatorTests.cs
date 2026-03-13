using NSubstitute;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.Apis;
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
            
            Sut = new AuthorizationCodeValidator(SecretQueryService);
        }
        
        public async Task<AuthorizationCodeValidationResult> ValidateAsync(
            AuthorizationCodeTokenCommand? command = null,
            ClientTokenData? tokenData = null,
            AuthorizationGrant? grant = null,
            ApiResource? api = null)
        {
            return await Sut.ValidateAsync(
                command ?? Command.Build(),
                tokenData ?? TokenData,
                grant ?? AuthorizationGrant.Build(),
                api ?? Api.Build());
        }

        public void WithSecret()
        {
            SecretQueryService
                .ValidateSecretAsync(Arg.Any<ClientId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(true);
        }
    }
    
    [Fact]
    public async Task ValidateAsync_WhenPkceIsPresent_ClientSecretIsIgnored()
    {
        var context = new TestContext();
        var command = context.Command
            .WithClientSecret("invalid-client-secret")
            .Build();
        
        var result = await context.ValidateAsync(command: command);
        Assert.Equal(DefaultValues.Scopes, result.Scope.ToString());
    }
    
    [Fact]
    public async Task ValidateAsync_WhenPkceIsMissing_ClientSecretIsUsed()
    {
        var context = new TestContext();
        var grant = context.AuthorizationGrant
            .WithPkce(null)
            .Build();
        
        var result = await context.ValidateAsync(grant: grant);
        Assert.Equal(DefaultValues.Scopes, result.Scope.ToString());
    }
    
    [Fact]
    public async Task ValidateAsync_WhenOpenIdScopes_ReturnValidationResult()
    {
        const string expected = "openid profile";
        
        var context = new TestContext();
        var grant = context.AuthorizationGrant
            .WithScopes(expected)
            .Build();

        var result = await context.ValidateAsync(grant: grant);
        Assert.Contains(expected, result.OidcScope.ToString());
    }

    [Fact]
    public async Task ValidateAsync_WithUsedAuthorizationGrant_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        var grant = context.AuthorizationGrant.Build();
        
        grant.Consume();
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => context.ValidateAsync());
    }
    
    [Fact]
    public async Task ValidateAsync_WithIncorrectClientId_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        var command = context.Command
            .WithClientId(ClientId.New())
            .Build();

        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => context.ValidateAsync(command: command));
    }

    [Fact]
    public async Task ValidateAsync_WithIncorrectRedirectUri_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        var command = context.Command
            .WithRedirectUri("https://invalid-uri.com")
            .Build();
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => context.ValidateAsync(command: command));       
    }
    
    [Fact]
    public async Task ValidateAsync_WithMissingCodeVerifier_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        var command = context.Command
            .WithCodeVerifier(null)
            .Build();
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => context.ValidateAsync(command: command));
    }
    
    [Fact]
    public async Task ValidateAsync_WithIncorrectCodeVerifier_ThrowsInvalidGrantException()
    {
        var context = new TestContext();
        var command = context.Command
            .WithCodeVerifier("invalid-code-verifier")
            .Build();
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => context.ValidateAsync(command: command));
    }

    [Fact]
    public async Task ValidateAsync_WithMissingClientSecret_ThrowsInvalidClientException()
    {
        // Set PKCE to null to force client secret for validation
        var context = new TestContext();
        var command = context.Command
            .WithClientSecret(null)
            .Build();

        var grant = context.AuthorizationGrant
            .WithPkce(null)
            .Build();
        
        await Assert.ThrowsAsync<InvalidClientException>(()
            => context.ValidateAsync(command: command, grant: grant));       
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidClientSecret_ThrowsInvalidClientException()
    {
        // Set PKCE to null to force client secret for validation
        var context = new TestContext();
        var grant = context.AuthorizationGrant
            .WithPkce(null)
            .Build();

        context.SecretQueryService
            .ValidateSecretAsync(Arg.Any<ClientId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        
        await Assert.ThrowsAsync<InvalidClientException>(()
            => context.ValidateAsync(grant: grant));
    }
}