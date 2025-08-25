using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Tokens;

public interface IJwtTokenGenerator
{
    string GenerateToken(Client client, Audience audience, IEnumerable<Scope> scopes, SigningKey signingKey);
}