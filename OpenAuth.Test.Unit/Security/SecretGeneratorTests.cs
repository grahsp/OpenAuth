using OpenAuth.Application.Security.Secrets;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Security;
using OpenAuth.Infrastructure.Security.Secrets;

namespace OpenAuth.Test.Unit.Security;

public class SecretGeneratorTests
{
    private readonly ISecretGenerator _gen = new SecretGenerator();

    [Fact]
    public void Generate_Default_IsBase64Url_And_LongEnough()
    {
        var s = _gen.Generate(); // default 32 bytes

        Assert.False(string.IsNullOrWhiteSpace(s));
        Assert.DoesNotContain("+", s);
        Assert.DoesNotContain("/", s);
        Assert.DoesNotContain("=", s);
        // 32 bytes -> base64url is typically 43 chars (unpadded)
        Assert.True(s.Length >= 43, $"Expected >=43 chars, got {s.Length}");
    }

    [Fact]
    public void Generate_SizeTooSmall_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _gen.Generate(15));
    }

    [Fact]
    public void Generate_TwoCalls_AreDifferent()
    {
        var a = _gen.Generate();
        var b = _gen.Generate();
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Generate_BiggerSize_YieldsLongerString()
    {
        var s32 = _gen.Generate(32);
        var s64 = _gen.Generate(64);
        Assert.True(s64.Length > s32.Length);
    }
}