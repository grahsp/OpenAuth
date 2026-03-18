using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Commands.CreateClient;

public sealed record CreateM2MClientCommand(ClientName Name, ApiResourceId ApiResourceId, ScopeCollection Scopes) : ICommand<CreateClientResult>;