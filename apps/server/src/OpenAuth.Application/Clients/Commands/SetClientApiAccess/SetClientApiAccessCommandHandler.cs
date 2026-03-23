using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;

namespace OpenAuth.Application.Clients.Commands.SetClientApiAccess;

public sealed class SetClientApiAccessCommandHandler(IApiResourceRepository apiRepository, IClientRepository clientRepository,TimeProvider time)
	: ICommandHandler<SetClientApiAccessCommand>
{
	public async Task HandleAsync(SetClientApiAccessCommand command, CancellationToken ct)
	{
        var client = await clientRepository.GetByIdAsync(command.ClientId, ct)
            ?? throw new ClientNotFoundException(command.ClientId);

        // TODO: should be a readonly query of allowed scopes instead.
        var api = await apiRepository.GetByIdAsync(command.ApiResourceId, ct)
            ?? throw new ApiResourceNotFoundException(command.ApiResourceId);

        var allowedScopes = api.Permissions
	        .Select(permission => permission.Scope)
	        .ToHashSet();
        
        if (!command.Scopes.IsSubsetOf(allowedScopes))
            throw new InvalidScopeException("One or more scopes requested are not allowed for the API.");
        
        client.SetApiAccess(api.Id, command.Scopes, time.GetUtcNow());
        await clientRepository.SaveChangesAsync(ct);
	}
}