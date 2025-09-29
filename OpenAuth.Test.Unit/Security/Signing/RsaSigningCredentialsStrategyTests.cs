using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Extensions;
using OpenAuth.Infrastructure.Security.Signing;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Security.Signing;

public class RsaSigningCredentialsStrategyTests
{
    private readonly RsaSigningCredentialsStrategy _strategy = new();
    private readonly TimeProvider _time = new FakeTimeProvider();
    
    [Fact]
    public void GetSigningCredentials_Throws_WhenKeyTypeMismatch()
    {
        var signingKey = TestSigningKey.CreateHmacSigningKey(_time);
        
        Assert.Throws<InvalidOperationException>(() =>
            _strategy.GetSigningCredentials(signingKey.Id.ToString(), signingKey.KeyMaterial));
    }

    [Fact]
    public void GetSigningCredentials_ReturnsSigningCredentials_WithExpectedProperties()
    {
        const string expectedKid = "test-kid";
        const SigningAlgorithm alg = SigningAlgorithm.RS256;
        
        var signingKey = TestSigningKey.CreateRsaSigningKey(_time, algorithm: alg);
        var credentials = _strategy.GetSigningCredentials(expectedKid, signingKey.KeyMaterial);

        Assert.NotNull(credentials);
        Assert.Equal(expectedKid, credentials.Key.KeyId);
        Assert.Equal(alg.ToSecurityString(), credentials.Algorithm);
    }
}