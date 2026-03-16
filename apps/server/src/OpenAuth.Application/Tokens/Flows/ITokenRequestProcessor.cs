using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public interface ITokenRequestProcessor
{
    GrantType GrantType { get; }
    Task<TokenContext> ProcessAsync(TokenCommand command, ClientTokenData tokenData, CancellationToken ct = default);
}