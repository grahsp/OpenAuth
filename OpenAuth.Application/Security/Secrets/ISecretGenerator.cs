namespace OpenAuth.Application.Security.Secrets;

public interface ISecretGenerator
{
    string Generate();
    string Generate(int byteLength);
}