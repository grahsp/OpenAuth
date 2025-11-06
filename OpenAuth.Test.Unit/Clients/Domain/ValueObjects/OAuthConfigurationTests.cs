using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class OAuthConfigurationTests
{
    private static OAuthConfiguration CreateDefaultConfig()
        => new OAuthConfigurationBuilder()
            .Build();
    
    private static NewAudience CreateAudience(string name = "test", string scopes = "") =>
        new(AudienceName.Create(name), ScopeCollection.Parse(scopes));
    
    public class Constructor : OAuthConfigurationTests
    {
        private readonly RedirectUri _defaultRedirectUri =
            RedirectUri.Create("https://example.com/callback");

        [Fact]
        public void WhenAudiencesEmpty_ThrowsException()
        {
            var grantTypes = new[] { GrantType.ClientCredentials };

            Assert.Throws<InvalidOperationException>(() =>
                new OAuthConfiguration([], grantTypes, [], false));
        }

        [Fact]
        public void WhenGrantTypesEmpty_ThrowsException()
        {
            var audiences = new[] { CreateAudience() };

            Assert.Throws<InvalidOperationException>(() =>
                new OAuthConfiguration(audiences, [], [], false));
        }

        [Fact]
        public void WhenMixingPublicAndConfidentialGrantTypes_ThrowsException()
        {
            var audiences = new[] { CreateAudience() };
            var grants = new[] { GrantType.ClientCredentials, GrantType.RefreshToken };

            Assert.Throws<InvalidOperationException>(() =>
                new OAuthConfiguration(audiences, grants, [], false));
        }

        [Fact]
        public void WhenRequiresRedirectUriAndNoneProvided_ThrowsException()
        {
            var audiences = new[] { CreateAudience() };
            var grants = new[] { GrantType.AuthorizationCode };

            Assert.Throws<InvalidOperationException>(() =>
                new OAuthConfiguration(audiences, grants, [], true));
        }

        [Fact]
        public void WhenRequiresRedirectUriAndProvided_Succeeds()
        {
            var audiences = new[] { CreateAudience() };
            var grants = new[] { GrantType.AuthorizationCode };
            var redirectUris = new[] { _defaultRedirectUri };

            var config = new OAuthConfiguration(audiences, grants, redirectUris, true);

            Assert.Single(config.RedirectUris);
        }

        [Fact]
        public void WhenPkceDisabledForPublicClient_ThrowsException()
        {
            var audiences = new[] { CreateAudience() };
            var grants = new[] { GrantType.RefreshToken };

            Assert.Throws<InvalidOperationException>(() =>
                new OAuthConfiguration(audiences, grants, [_defaultRedirectUri], false));
        }

        [Fact]
        public void WhenPkceDisabledForConfidentialClient_Succeeds()
        {
            var audiences = new[] { CreateAudience() };
            var grants = new[] { GrantType.ClientCredentials };

            var config = new OAuthConfiguration(audiences, grants, [], false);

            Assert.False(config.RequirePkce);
        }

        [Fact]
        public void WhenDuplicatesProvided_RemoveThem()
        {
            var audiences = new[]
            {
                CreateAudience("api"),
                CreateAudience("api")
            };
            var grants = new[] { GrantType.ClientCredentials, GrantType.ClientCredentials };

            var config = new OAuthConfiguration(audiences, grants, [], false);

            Assert.Single(config.Audiences);
            Assert.Single(config.GrantTypes);
        }
    } 
    
    public class SetAudiences : OAuthConfigurationTests
    {
        [Fact]
        public void WhenValid_ReturnsNewInstance()
        {
            var config = CreateDefaultConfig();

            var updated = config.SetAudiences([CreateAudience()]);
        
            Assert.NotSame(updated, config);
            Assert.Single(updated.Audiences);
        }

        [Fact]
        public void WhenSame_ReturnsSameInstance()
        {
            var audience = CreateAudience();
            var config = new OAuthConfigurationBuilder()
                .WithAudiences(audience)
                .Build();

            var updated = config.SetAudiences([audience]);
        
            Assert.Same(updated, config);              
        }
    
        [Fact]
        public void WhenDuplicates_RemoveThem()
        {
            var a = CreateAudience();
            var b = CreateAudience();
            var config = CreateDefaultConfig();
        
            var updated = config.SetAudiences([a, b]);
        
            Assert.Single(updated.Audiences);
        }
    
        [Fact]
        public void WhenNull_ThrowsException()
        {
            var config = CreateDefaultConfig();

            Assert.Throws<ArgumentNullException>(()
                => config.SetAudiences(null!));
        }
    
        [Fact]
        public void WhenEmpty_ThrowsException()
        {
            var config = CreateDefaultConfig();

            Assert.Throws<InvalidOperationException>(()
                => config.SetAudiences([]));       
        }
    }

    public class SetGrantTypes
    {
        [Fact]
        public void WhenConfidentialClientWithClientCredentialsOnly_Succeeds()
        {
            var config = CreateDefaultConfig();
            
            var updated = config
                .SetGrantTypes([GrantType.ClientCredentials]);
        
            Assert.NotSame(updated, config);
            Assert.Single(updated.GrantTypes);
        }
        
        [Fact]
        public void WhenPublicClientWithRefreshTokenOnly_Succeeds()
        {
            var config = CreateDefaultConfig();
            
            var updated = config
                .SetGrantTypes([GrantType.RefreshToken]);
        
            Assert.NotSame(updated, config);
            Assert.Single(updated.GrantTypes);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WhenAuthCodeInConfidentialClientPkceRequiredIsOptional_Succeeds(bool requirePkce)
        {
            var grants = new[] { GrantType.AuthorizationCode, GrantType.ClientCredentials };
            var config = new OAuthConfigurationBuilder()
                .WithRequirePkce(requirePkce)
                .Build();
            
            var updated = config
                .SetGrantTypes(grants);
        
            Assert.NotSame(updated, config);
            Assert.Equal(grants.Length, updated.GrantTypes.Count);
        }

        [Fact]
        public void WhenAuthCodeInPublicClientWithPkce_Succeeds()
        {
            var grants = new[] { GrantType.AuthorizationCode, GrantType.RefreshToken };
            var config = new OAuthConfigurationBuilder()
                .WithRequirePkce(true)
                .Build();
            
            var updated = config.SetGrantTypes(grants);
            
            Assert.NotSame(config, updated);
            Assert.Equal(grants.Length, updated.GrantTypes.Count);
        }
        
        [Fact]
        public void WhenAuthCodeInPublicClientWithoutPkce_ThrowsException()
        {
            var config = new OAuthConfigurationBuilder()
                .WithRequirePkce(false)
                .Build();
            
            Assert.Throws<InvalidOperationException>(()
                => config.SetGrantTypes([GrantType.AuthorizationCode, GrantType.RefreshToken]));
        }

        [Fact]
        public void WhenExplicitPublicAndConfidentialGrantTypes_ThrowsException()
        {
            var config = CreateDefaultConfig();
            
            Assert.Throws<InvalidOperationException>(()
                => config.SetGrantTypes([GrantType.ClientCredentials, GrantType.RefreshToken]));           
        }

        [Fact]
        public void WhenRedirectUriRequiredAndExists_Succeeds()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.RefreshToken)
                .WithRedirectUris("https://example.com/callback")
                .Build();
            
            var updated =
                config.SetGrantTypes([GrantType.AuthorizationCode]);
        
            Assert.NotSame(updated, config);           
        }

        [Fact]
        public void WhenRedirectUriNotRequiredAndDoesNotExist_Succeeds()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.RefreshToken)
                .Build();
            
            var updated =
                config.SetGrantTypes([GrantType.ClientCredentials]);
        
            Assert.NotSame(updated, config);
        }

        [Fact]
        public void WhenDuplicates_RemoveThem()
        {
            var config = CreateDefaultConfig();
            
            var updated =
                config.SetGrantTypes([GrantType.ClientCredentials, GrantType.ClientCredentials]);
        
            Assert.Single(updated.GrantTypes);
        }

        [Fact]
        public void WhenSameGrantTypes_ReturnsSameInstance()
        {
            var grantTypes = new[] { GrantType.ClientCredentials };
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(grantTypes)
                .Build();
            
            var updated =
                config.SetGrantTypes(grantTypes);
        
            Assert.Same(updated, config);
        }
        
        [Fact]
        public void WhenRequiresRedirectUriAndNoneExists_ThrowsException()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.ClientCredentials)
                .WithRedirectUris(Array.Empty<RedirectUri>())
                .Build();
            
            Assert.Throws<InvalidOperationException>(()
                => config.SetGrantTypes([GrantType.AuthorizationCode]));
        }
        
        [Fact]
        public void WhenNull_ThrowsException()
        {
            var config = CreateDefaultConfig();
            
            Assert.Throws<ArgumentNullException>(()
                => config.SetGrantTypes(null!));
        }

        [Fact]
        public void WhenEmpty_ThrowsException()
        {
            var config = CreateDefaultConfig();
            
            Assert.Throws<InvalidOperationException>(()
                => config.SetGrantTypes([]));
        }
    }

    public class SetRedirectUris
    {
        [Fact]
        public void WhenSameUris_ReturnsSameInstance()
        {
            var uris = new[]
            {
                RedirectUri.Create("https://example.com/callback"),
                RedirectUri.Create("https://test.com/callback")
            };
            var config = new OAuthConfigurationBuilder()
                .WithRedirectUris(uris)
                .Build();

            var updated = config.SetRedirectUris(uris);
        
            Assert.Same(updated, config);
            Assert.Equal(uris, updated.RedirectUris);
        }

        [Fact]
        public void WhenGrantTypeRequiresRedirectUriAndExists_Succeeds()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.AuthorizationCode)
                .Build();

            var updated =
                config.SetRedirectUris([RedirectUri.Create("https://test.com/callback")]);
        
            Assert.NotSame(updated, config);
            Assert.Single(updated.RedirectUris);           
        }
        
        [Fact]
        public void WhenEmptyAndGrantTypeDoesNotRequireRedirectUri_Succeeds()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.ClientCredentials)
                .Build();

            var updated = config.SetRedirectUris([]);
        
            Assert.NotSame(updated, config);
            Assert.Empty(updated.RedirectUris);
        }
        
        [Fact]
        public void WhenEmptyAndGrantTypeRequiresRedirectUri_ThrowsException()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.AuthorizationCode)
                .Build();

            Assert.Throws<InvalidOperationException>(()
                => config.SetRedirectUris([]));
        }

        [Fact]
        public void WhenDuplicates_RemoveThem()
        {
            var config = CreateDefaultConfig();
            var uris = new[]
            {
                RedirectUri.Create("https://test.com/callback"),
                RedirectUri.Create("https://test.com/callback")
            };

            var updated = config.SetRedirectUris(uris);
            
            Assert.NotSame(config, updated);
            Assert.Single(updated.RedirectUris);
        }
        
        [Fact]
        public void WhenNull_ThrowsException()
        {
            var config = CreateDefaultConfig();
            
            Assert.Throws<ArgumentNullException>(()
                => config.SetRedirectUris(null!));
        }
    }

    public class SetRequirePkce
    {
        [Fact]
        public void WhenPublicClientWithAuthCode_DisablingPkce_ThrowsException()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.AuthorizationCode)
                .WithRequirePkce(true)
                .Build();
        
            Assert.Throws<InvalidOperationException>(()
                => config.SetRequirePkce(false));
        }

        [Fact]
        public void WhenPublicClientWithAuthCode_EnablingPkce_Succeeds()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.AuthorizationCode)
                .WithRequirePkce(false)
                .Build();
        
            var updated = config.SetRequirePkce(true);
        
            Assert.NotSame(config, updated);
            Assert.True(updated.RequirePkce);
        }

        [Fact]
        public void WhenConfidentialClient_DisablingPkce_Succeeds()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.ClientCredentials)
                .WithRequirePkce(true)
                .Build();

            var updated = config.SetRequirePkce(false);

            Assert.NotSame(config, updated);
            Assert.False(updated.RequirePkce);
        }

        [Fact]
        public void WhenConfidentialClient_EnablingPkce_Succeeds()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.ClientCredentials)
                .WithRequirePkce(false)
                .Build();

            var updated = config.SetRequirePkce(true);

            Assert.NotSame(config, updated);
            Assert.True(updated.RequirePkce);
        }

        [Fact]
        public void WhenMixedPublicAndConfidential_DisablingPkce_Succeeds()
        {
            var config = new OAuthConfigurationBuilder()
                .WithGrantTypes(GrantType.AuthorizationCode, GrantType.ClientCredentials)
                .WithRequirePkce(true)
                .Build();

            var updated = config.SetRequirePkce(false);

            Assert.NotSame(config, updated);
            Assert.False(updated.RequirePkce);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WhenSameValue_ReturnsSameInstance(bool requirePkce)
        {
            var config = new OAuthConfigurationBuilder()
                .WithRequirePkce(requirePkce)
                .Build();

            var updated = config.SetRequirePkce(requirePkce);

            Assert.Same(config, updated);
        }
    }
}