using DotEnv.SecretManager.CLI.Commands;
using DotEnv.SecretManager.CLI.Infrastructure;
using DotEnv.SecretManager.Core.Services;

namespace DotEnv.SecretManager.CLI;

class Program
{
    private static readonly string Version = "1.1.0";

    static async Task<int> Main(string[] args)
    {
        try
        {
            var services = CreateServices();
            var commandRunner = new CommandRunner(services, Version);

            return await commandRunner.RunAsync(args);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Unexpected error: {ex.Message}");
            return 1;
        }
    }

    private static AppServices CreateServices()
    {
        var encryptionService = new AesEncryptionService();
        var envFileService = new EnvFileService();
        var secretManager = new Core.Services.SecretManager(encryptionService, envFileService);

        return new AppServices(encryptionService, envFileService, secretManager);
    }
}