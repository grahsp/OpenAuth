using OpenAuth.Application.Audiences.Dtos;
using OpenAuth.Application.Audiences.Mappings;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Audiences.Services;

public class AudienceService : IAudienceService
{
    private readonly IClientRepository _repository;
    private readonly TimeProvider _time;
    
    public AudienceService(IClientRepository repository, TimeProvider time)
    {
        _repository = repository;
        _time = time;
    }
    
    public async Task<AudienceInfo> AddAudienceAsync(ClientId id, AudienceName name,
        CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken) ??
                     throw new InvalidOperationException("Client not found.");

        var audience = client.AddAudience(name, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);
        
        return audience.ToAudienceInfo();
    }
    
    public async Task RemoveAudienceAsync(ClientId id, AudienceName name, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken) ??
                     throw new InvalidOperationException("Client not found.");

        client.RemoveAudience(name, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AudienceInfo> SetScopesAsync(ClientId id, AudienceName name, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        var audience = client.SetScopes(name, scopes, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);

        return audience.ToAudienceInfo();
    }
    
    public async Task<AudienceInfo> GrantScopesAsync(ClientId id, AudienceName name, IEnumerable<Scope> scopes,
        CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        var audience = client.GrantScopes(name, scopes, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);

        return audience.ToAudienceInfo();
    }

    public async Task<AudienceInfo> RevokeScopesAsync(ClientId id, AudienceName name, IEnumerable<Scope> scopes,
        CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        var audience = client.RevokeScopes(name, scopes, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);

        return audience.ToAudienceInfo();
    }
}