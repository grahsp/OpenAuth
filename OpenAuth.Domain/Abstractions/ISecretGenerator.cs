namespace OpenAuth.Domain.Abstractions;

public interface ISecretGenerator
{
    string Generate();
    string Generate(int byteLength);
}