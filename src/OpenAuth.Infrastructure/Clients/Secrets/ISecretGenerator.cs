namespace OpenAuth.Infrastructure.Clients.Secrets;

public interface ISecretGenerator
{
    string Generate();
    string Generate(int byteLength);
}