using OpenAuth.Domain.Clients.Audiences;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class OAuthConfigurationTests
{
    private static OAuthConfiguration CreateDefaultConfig()
        => new OAuthConfigurationBuilder()
            .Build();
    
    public class SetAudiences
    {
        private static Audience CreateAudience(string name)
            => new AudienceBuilder()
                .WithName(name)
                .Build();
        
        [Fact]
        public void WhenValid_ReturnsNewInstance()
        {
            var audience = CreateAudience("test");
            var config = CreateDefaultConfig();

            var updated = config.SetAudiences([audience]);
        
            Assert.NotSame(updated, config);
            Assert.Single(updated.Audiences);
        }

        [Fact]
        public void WhenSame_ReturnsSameInstance()
        {
            var audience = CreateAudience("test");
            var config = new OAuthConfigurationBuilder()
                .WithAudiences(audience)
                .Build();

            var updated = config.SetAudiences([audience]);
        
            Assert.Same(updated, config);              
        }
    
        [Fact]
        public void WhenDuplicates_RemoveThem()
        {
            var a = CreateAudience("test");
            var b = CreateAudience("test");
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
}