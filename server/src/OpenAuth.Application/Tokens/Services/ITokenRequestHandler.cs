using OpenAuth.Application.Tokens.Dtos;

namespace OpenAuth.Application.Tokens.Services;

public interface ITokenRequestHandler
{
    Task<TokenResult> HandleAsync(TokenCommand command, CancellationToken ct = default);
}