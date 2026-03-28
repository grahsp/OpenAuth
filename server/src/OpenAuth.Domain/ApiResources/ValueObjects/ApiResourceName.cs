namespace OpenAuth.Domain.ApiResources.ValueObjects;

public sealed record ApiResourceName(string Value)
{
	public override string ToString() => Value;
}