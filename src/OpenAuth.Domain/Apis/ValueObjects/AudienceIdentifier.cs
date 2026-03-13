using System.Diagnostics.CodeAnalysis;

namespace OpenAuth.Domain.Apis.ValueObjects;

public sealed record AudienceIdentifier
{
	public string Value { get; }

	public AudienceIdentifier(string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);
		
		Value = value;
	}
	
	public static AudienceIdentifier Create(string value)
	{
		if (!TryCreate(value, out var audience))
			throw new ArgumentException($"Invalid audience identifier: {value}", nameof(value));

		return audience;
	}
	
	public static bool TryCreate(string? value, [NotNullWhen(true)] out AudienceIdentifier? audience)
	{
		audience = null;

		if (string.IsNullOrWhiteSpace(value))
			return false;

		try
		{
			audience = new AudienceIdentifier(value);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}