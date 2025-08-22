using OpenAuth.Domain.Abstractions;

namespace OpenAuth.Application.Tests.Stubs;

internal sealed class FakeGenerator : ISecretGenerator
{
    private readonly string _value;
    public FakeGenerator(string v) => _value = v;

    public string Generate() => Generate(32);
    public string Generate(int byteLength) => _value;
}