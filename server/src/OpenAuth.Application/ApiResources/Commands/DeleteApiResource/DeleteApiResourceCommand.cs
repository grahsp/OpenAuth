using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;

namespace OpenAuth.Application.ApiResources.Commands.DeleteApiResource;

public sealed record DeleteApiResourceCommand(ApiResourceId Id) : ICommand;