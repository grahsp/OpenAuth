namespace OpenAuth.Api.Dtos;

public record CreateClientResponse {
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required string SecretId { get; init; }
    public required DateTimeOffset SecretExpiresAt { get; init; }
}