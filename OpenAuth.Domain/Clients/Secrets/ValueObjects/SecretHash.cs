using OpenAuth.Domain.Shared.Interfaces;

namespace OpenAuth.Domain.Clients.Secrets.ValueObjects;

public readonly record struct SecretHash
{
    public string Value { get; }

    // TODO: make private (add FromHash method?)
    public SecretHash(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }

    public static SecretHash Create(string plainText, IHasher hasher)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        ArgumentNullException.ThrowIfNull(hasher);

        var hashed = hasher.Hash(plainText);
        return new SecretHash(hashed);
    }

    public bool Verify(string plainText, IHasher hasher)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        ArgumentNullException.ThrowIfNull(hasher);

        return hasher.Verify(plainText, Value);
    }

    public override string ToString() => Value;
}