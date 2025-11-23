using System.Security.Claims;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Oidc;

public interface IOidcUserClaimsProvider
{
    Task<IEnumerable<Claim>> GetUserClaimsAsync(string subjectId, ScopeCollection scopes);
}