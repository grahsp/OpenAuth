using OpenAuth.Application.Audiences.Dtos;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Audiences.Services;

// public interface IAudienceService
// {
//     Task<AudienceInfo> AddAudienceAsync(ClientId id, AudienceName name, CancellationToken cancellationToken = default);
//     Task RemoveAudienceAsync(ClientId id, AudienceName name, CancellationToken cancellationToken = default);
//     Task<AudienceInfo> SetScopesAsync(ClientId id, AudienceName name, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
//     Task<AudienceInfo> GrantScopesAsync(ClientId id, AudienceName name, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
//     Task<AudienceInfo> RevokeScopesAsync(ClientId id, AudienceName name, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
//
// }