namespace OpenAuth.Domain.AuthorizationGrant.ValueObjects;

public sealed record PkceData(string CodeChallenge, string CodeChallengeMethod);