namespace OpenAuth.Api.Dtos;

public record DiscoveryResult(string Issuer, string JwksUri, string TokenEndpoint);