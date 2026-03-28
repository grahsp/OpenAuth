using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public abstract class TokenRequestProcessor<TRequest> : ITokenRequestProcessor where TRequest : TokenCommand
{
    public abstract GrantType GrantType { get; }
    protected abstract Task<TokenContext> ProcessAsync(TRequest request, ClientTokenData tokenData, CancellationToken ct = default);
    
    public async Task<TokenContext> ProcessAsync(TokenCommand command, ClientTokenData tokenData, CancellationToken ct = default)
    {
        if (command is not TRequest typedRequest)
            throw new InvalidOperationException(
                $"Grant type '{GrantType}' does not support request type '{command.GetType().Name}'.");
        
        return await ProcessAsync(typedRequest, tokenData, ct);
    }
}