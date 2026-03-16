using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;

namespace OpenAuth.Application.ApiResources.Commands.AddApiResourcePermissions;

public sealed record AddApiResourcePermissionCommand(ApiResourceId Id, IEnumerable<Permission> Permissions) : ICommand;