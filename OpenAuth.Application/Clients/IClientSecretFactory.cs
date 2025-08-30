using OpenAuth.Domain.Entities;

namespace OpenAuth.Application.Clients;

public interface IClientSecretFactory
{
    (ClientSecret secret, string plain) Create(DateTime? expiresAt = null);
}