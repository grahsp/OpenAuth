using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public interface IClientSecretFactory
{
    SecretCreationResult Create(DateTime? expiresAt = null);
}