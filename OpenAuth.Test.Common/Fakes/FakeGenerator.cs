using OpenAuth.Application.Secrets;
using OpenAuth.Application.Secrets.Interfaces;

namespace OpenAuth.Test.Common.Fakes;

public class FakeGenerator : ISecretGenerator
{
    private readonly string _value;
    public FakeGenerator(string v) => _value = v;

    public string Generate() => Generate(32);
    public string Generate(int byteLength) => _value;
}