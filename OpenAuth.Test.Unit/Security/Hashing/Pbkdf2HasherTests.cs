using OpenAuth.Application.Security.Hashing;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Infrastructure.Security.Hashing;

namespace OpenAuth.Test.Unit.Security.Hashing;

public class Pbkdf2HasherTests
{
    private readonly IHasher _hasher = new Pbkdf2Hasher(iterations: 60_000); // keep test-time reasonable

    [Fact]
    public void Hash_Then_Verify_Succeeds()
    {
        var plain = "super-secret-🗝️";
        var hash = _hasher.Hash(plain);

        Assert.True(_hasher.Verify(plain, hash));
    }

    [Fact]
    public void Verify_WrongSecret_Fails()
    {
        var hash = _hasher.Hash("correct horse battery staple");
        Assert.False(_hasher.Verify("wrong secret", hash));
    }

    [Fact]
    public void Hash_Format_HasVersionIterationsSaltHash()
    {
        var hash = _hasher.Hash("x");
        var value = hash;

        Assert.StartsWith("v1$", value, StringComparison.Ordinal);
        var parts = value.Split('$');
        Assert.Equal(4, parts.Length);
        Assert.True(int.TryParse(parts[1], out var iters) && iters > 0);
        Assert.False(string.IsNullOrWhiteSpace(parts[2])); // salt b64
        Assert.False(string.IsNullOrWhiteSpace(parts[3])); // key b64
    }

    [Fact]
    public void Verify_TamperedIterations_Fails()
    {
        var h = _hasher.Hash("x");
        var parts = h.Split('$');
        parts[1] = "1"; // ridiculous
        var tampered = new SecretHash(string.Join('$', parts));
        Assert.False(_hasher.Verify("x", tampered.Value));
    }

    [Fact]
    public void Verify_TamperedSalt_Fails()
    {
        var h = _hasher.Hash("x");
        var parts = h.Split('$');
        parts[2] = "!!!not-base64!!!";
        var tampered = new SecretHash(string.Join('$', parts));
        Assert.False(_hasher.Verify("x", tampered.Value));
    }

    [Fact]
    public void Verify_TamperedHash_Fails()
    {
        var h = _hasher.Hash("x");
        var parts = h.Split('$');
        // Change last char safely
        parts[3] = parts[3].Substring(0, parts[3].Length - 1) + (parts[3].EndsWith("A") ? "B" : "A");
        var tampered = new SecretHash(string.Join('$', parts));
        Assert.False(_hasher.Verify("x", tampered.Value));
    }

    [Fact]
    public void Ctor_TooFewIterations_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Pbkdf2Hasher(iterations: 9_000));
    }

    [Fact]
    public void Hash_EmptyOrWhitespace_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _hasher.Hash(null!));
        Assert.Throws<ArgumentException>(() => _hasher.Hash("   "));
    }
}