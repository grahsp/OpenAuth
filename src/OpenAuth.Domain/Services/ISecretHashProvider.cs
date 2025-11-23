using OpenAuth.Domain.Services.Dtos;

namespace OpenAuth.Domain.Services;

public interface ISecretHashProvider
{
    SecretCreationResult Create();
}