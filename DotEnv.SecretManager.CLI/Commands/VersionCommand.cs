using DotEnv.SecretManager.CLI.Infrastructure;

namespace DotEnv.SecretManager.CLI.Commands;

public static class VersionCommand
{
    public static int ShowVersion(string version)
    {
        Console.WriteLine($"DotEnv Secret Manager v{version}");
        Console.WriteLine("Secure .env file encryption with AES-256");
        Console.WriteLine();
        ConsoleHelper.WriteInfo("Security: AES-256-CBC with PBKDF2-SHA256 (10,000 iterations)");
        ConsoleHelper.WriteInfo("Runtime: .NET 9.0");
        ConsoleHelper.WriteInfo("Website: https://github.com/iiSmitty/envcrypt");

        return 0;
    }
}