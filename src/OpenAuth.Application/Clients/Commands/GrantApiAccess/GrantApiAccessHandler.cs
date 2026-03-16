using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;

namespace OpenAuth.Application.Clients.Commands.GrantApiAccess;

public sealed class GrantApiAccessHandler(IApiResourceRepository apiRepository, IClientRepository clientRepository,TimeProvider time)
	: ICommandHandler<GrantApiAccessCommand>
{
	public async Task HandleAsync(GrantApiAccessCommand command, CancellationToken ct)
	{
        var client = await clientRepository.GetByIdAsync(command.ClientId, ct)
            ?? throw new ClientNotFoundException(command.ClientId);

        var api = await apiRepository.GetByIdAsync(command.ApiResourceId, ct)
            ?? throw new ApiResourceNotFoundException(command.ApiResourceId);

        var allowedScopes = api.Permissions
	        .Select(permission => permission.Scope)
	        .ToHashSet();
        
        if (!command.Scopes.IsSubsetOf(allowedScopes))
            throw new InvalidScopeException("One or more scopes requested are not allowed for the API.");
        
        client.GrantApiAccess(api.Id, command.Scopes, time.GetUtcNow());
        await clientRepository.SaveChangesAsync(ct);
	}
}