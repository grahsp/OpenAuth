using OpenAuth.Domain.Clients;

namespace OpenAuth.Application.Clients.Commands.CreateClient;

public sealed record CreateClientResult(Client Client, string? PlainTextSecret);