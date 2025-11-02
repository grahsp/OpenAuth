namespace OpenAuth.Domain.Clients.ValueObjects;

public record ClientIdentity(
    ClientId Id,
    ClientName Name)
{
    public ClientIdentity SetName(ClientName name)
    {
        ArgumentNullException.ThrowIfNull(name);
            
        return this with { Name = name };
    }
}