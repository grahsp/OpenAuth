using OpenAuth.Application.Dtos;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Infrastructure.Persistence.QuerySpecifications;

public static class ClientQuerySpecifications
{
    public static IQueryable<ClientInfo> ToClientInfo(this IQueryable<Client> query)
        => query.Select(c => new ClientInfo
        (
            c.Id,
            c.Name,
            c.CreatedAt,
            c.UpdatedAt
        ));

    public static IQueryable<ClientDetails> ToClientDetails(this IQueryable<Client> query)
        => query.Select(c => new ClientDetails(
            c.Id,
            c.Name,
            c.CreatedAt,
            c.UpdatedAt,
            c.Secrets
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SecretInfo(
                    s.Id,
                    s.CreatedAt,
                    s.ExpiresAt,
                    s.RevokedAt
                )),
            c.Audiences.Select(a => new AudienceInfo(
                a.Value,
                a.Scopes
                    .Select(s => s.Value)))
            ));
}