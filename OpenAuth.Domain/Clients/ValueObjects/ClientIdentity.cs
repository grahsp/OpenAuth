namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record ClientIdentity
{
    public ClientId Id { get; }
    public ClientName Name { get; private init; }
    
    public ClientIdentity(ClientId id, ClientName name)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        
        if (id.Value == Guid.Empty)
            throw new ArgumentException("Client ID cannot be empty.", nameof(id));
        
        Id = id;
        Name = name;
    }
    
    public ClientIdentity SetName(ClientName name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Name == name ? this : this with { Name = name };
    }
}