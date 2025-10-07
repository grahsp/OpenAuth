namespace OpenAuth.Application.Secrets.Interfaces;

public interface ISecretGenerator
{
    string Generate();
    string Generate(int byteLength);
}