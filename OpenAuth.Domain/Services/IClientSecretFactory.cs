using OpenAuth.Domain.Entities;

namespace OpenAuth.Domain.Services;

public interface IClientSecretFactory
{
    (ClientSecret secret, string plain) Create(DateTime? expiresAt = null);
}