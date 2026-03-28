using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Commands.CreateClient;

public sealed record CreateSpaClientCommand(ClientName Name) : ICommand<CreateClientResult>;