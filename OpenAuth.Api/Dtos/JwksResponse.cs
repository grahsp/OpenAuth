namespace OpenAuth.Api.Dtos;

public record JwksResponse(IEnumerable<Jwk> Keys);