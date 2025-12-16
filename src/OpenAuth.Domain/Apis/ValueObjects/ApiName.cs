using OpenAuth.Domain.Shared.ValueObjects;

namespace OpenAuth.Domain.Apis.ValueObjects;

public sealed record ApiName(string Value) : Name(Value, 3, 32);