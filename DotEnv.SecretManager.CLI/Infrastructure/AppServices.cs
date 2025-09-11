using DotEnv.SecretManager.Core.Interfaces;

namespace DotEnv.SecretManager.CLI.Infrastructure;

public class AppServices
{
    public IEncryptionService EncryptionService { get; }
    public IEnvFileService EnvFileService { get; }
    public ISecretManager SecretManager { get; }

    public AppServices(
        IEncryptionService encryptionService,
        IEnvFileService envFileService,
        ISecretManager secretManager)
    {
        EncryptionService = encryptionService;
        EnvFileService = envFileService;
        SecretManager = secretManager;
    }
}