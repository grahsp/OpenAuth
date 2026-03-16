namespace OpenAuth.Domain.Apis.ValueObjects;

public readonly record struct AudienceIdentifier
{
	public string Value { get; }

	public AudienceIdentifier(string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);
		Value = value.Trim();
	}
	
	public static bool TryParse(string? value, out AudienceIdentifier audience)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			audience = default;
			return false;
		}

		audience = new AudienceIdentifier(value);
		return true;
	}

	public static implicit operator string(AudienceIdentifier id) => id.Value;
	public override string ToString() => Value;
}