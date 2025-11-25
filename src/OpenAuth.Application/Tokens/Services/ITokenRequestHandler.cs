using OpenAuth.Application.Tokens.Dtos;

namespace OpenAuth.Application.Tokens.Services;

public interface ITokenRequestHandler
{
    Task<TokenResult> IssueToken(TokenCommand command, CancellationToken ct = default);
}