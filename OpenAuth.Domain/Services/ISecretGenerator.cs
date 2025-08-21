namespace OpenAuth.Domain.Services;

public interface ISecretGenerator
{
    string Generate();
    string Generate(int byteLength);
}