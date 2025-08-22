using OpenAuth.Domain.Entities;

namespace OpenAuth.Domain.Abstractions;

public interface IClientSecretFactory
{
    (ClientSecret secret, string plain) Create(DateTime? expiresAt = null);
}