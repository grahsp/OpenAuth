using System.Security.Claims;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Oidc;

public interface IUserClaimsQueryService
{
    Task<IEnumerable<Claim>> GetUserClaimsAsync(string subjectId, ScopeCollection scopes);
}