using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.ApiResources.Commands.RemoveApiResourcePermissions;

public sealed record RemoveApiResourcePermissionsCommand(ApiResourceId Id, IEnumerable<Scope> Scopes) : ICommand;