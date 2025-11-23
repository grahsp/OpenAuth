using OpenAuth.Application.Tokens.Dtos;

namespace OpenAuth.Application.Tokens.Services;

public interface ITokenService
{
    Task<TokenGenerationResponse> IssueToken(TokenCommand command, CancellationToken ct = default);
}