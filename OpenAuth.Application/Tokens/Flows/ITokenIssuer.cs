using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public interface ITokenIssuer
{
    GrantType GrantType { get; }
    Task<TokenContext> IssueToken(TokenRequest request, CancellationToken ct = default);
}