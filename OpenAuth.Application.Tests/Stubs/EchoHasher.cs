using OpenAuth.Domain.Abstractions;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Tests.Stubs;

internal sealed class EchoHasher : ISecretHasher
{
    public SecretHash Hash(string plain) => new SecretHash("v1$echo$salt$" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plain)));
    public bool Verify(string input, SecretHash encoded) => encoded.Value.EndsWith(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input)), StringComparison.Ordinal);
}