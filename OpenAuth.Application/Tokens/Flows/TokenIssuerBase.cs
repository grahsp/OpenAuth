using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public abstract class TokenIssuerBase<TRequest> : ITokenIssuer where TRequest : TokenRequest
{
    public abstract GrantType GrantType { get; }
    protected abstract Task<TokenContext> IssueToken(TRequest request, CancellationToken ct = default);
    
    public async Task<TokenContext> IssueToken(TokenRequest request, CancellationToken ct = default)
    {
        if (request is not TRequest typedRequest)
            throw new InvalidOperationException(
                $"Grant type '{GrantType}' does not support request type '{request.GetType().Name}'.");
        
        return await IssueToken(typedRequest, ct);
    }
}