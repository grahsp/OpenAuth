namespace OpenAuth.Domain.Clients.Secrets.ValueObjects;

public readonly record struct SecretHash
{
    public string Value { get; }

    public SecretHash(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }

    public override string ToString() => Value;
}