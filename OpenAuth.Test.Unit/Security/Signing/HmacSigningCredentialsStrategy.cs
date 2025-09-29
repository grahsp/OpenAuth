using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Signing;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Security.Signing;

public class HmacSigningCredentialsStrategyTests
{
    private readonly HmacSigningCredentialsStrategy _strategy = new();
    private readonly TimeProvider _time = new FakeTimeProvider();
    
    [Fact]
    public void GetSigningCredentials_Throws_WhenKeyTypeMismatch()
    {
        var signingKey = TestSigningKey.CreateRsaSigningKey(_time);

        Assert.Throws<InvalidOperationException>(() =>
            _strategy.GetSigningCredentials(signingKey.Id.ToString(), signingKey.KeyMaterial));
    }

    [Fact]
    public void Create_ReturnsSigningCredentials_WithExpectedProperties()
    {
        const string expectedKid = "test-kid";
        const SigningAlgorithm alg = SigningAlgorithm.HS256;
        
        var signingKey = TestSigningKey.CreateHmacSigningKey(_time, algorithm: alg);
        var signingCredentials = _strategy.GetSigningCredentials(expectedKid, signingKey.KeyMaterial);

        Assert.Equal(alg.ToString(), signingCredentials.Algorithm);
        Assert.Equal(expectedKid, signingCredentials.Key.KeyId);
    }
}