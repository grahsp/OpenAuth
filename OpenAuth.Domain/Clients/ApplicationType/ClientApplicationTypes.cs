namespace OpenAuth.Domain.Clients.ApplicationType;

public static class ClientApplicationTypes
{
    public static readonly ClientApplicationType Spa = new SinglePageClientApplicationType();
    public static readonly ClientApplicationType M2M = new MachineToMachineClientApplicationType();
    
    public static ClientApplicationType Parse(string value)
        => value switch
        {
            "spa" => Spa,
            "m2m" => M2M,
            _ => throw new ArgumentException($"Invalid application type: {value}")
        };
}