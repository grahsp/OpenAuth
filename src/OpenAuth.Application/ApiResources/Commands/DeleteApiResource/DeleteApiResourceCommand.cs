using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Apis.ValueObjects;

namespace OpenAuth.Application.ApiResources.Commands.DeleteApiResource;

public sealed record DeleteApiResourceCommand(ApiResourceId Id) : ICommand;