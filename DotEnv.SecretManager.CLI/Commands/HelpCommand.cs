using DotEnv.SecretManager.CLI.Infrastructure;

namespace DotEnv.SecretManager.CLI.Commands;

public static class HelpCommand
{
    public static int ShowHelp(string version)
    {
        Console.WriteLine($"DotEnv Secret Manager v{version}");
        Console.WriteLine("Encrypt and decrypt .env files safely with AES-256");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("USAGE:");
        Console.WriteLine("  dotenv-encrypt <command> [options]");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("COMMANDS:");
        Console.WriteLine("  encrypt <file>     Encrypt a .env file");
        Console.WriteLine("  decrypt <file>     Decrypt an encrypted .env file");
        Console.WriteLine("  validate <file>    Validate that an encrypted file can be decrypted");
        Console.WriteLine("  info <file>        Show detailed information about a .env file");
        Console.WriteLine("  examples           Show usage examples");
        Console.WriteLine("  version            Show version information");
        Console.WriteLine("  help               Show this help message");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("GLOBAL OPTIONS:");
        Console.WriteLine("  --help, -h         Show help");
        Console.WriteLine("  --version, -v      Show version");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("ENCRYPT OPTIONS:");
        Console.WriteLine("  --output, -o       Output file path");
        Console.WriteLine("  --password, -p     Password (will prompt if not provided)");
        Console.WriteLine("  --force, -f        Overwrite existing files without prompt");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("DECRYPT OPTIONS:");
        Console.WriteLine("  --output, -o       Output file path");
        Console.WriteLine("  --password, -p     Password (will prompt if not provided)");
        Console.WriteLine("  --force, -f        Overwrite existing files without prompt");
        Console.WriteLine();

        ConsoleHelper.WriteDim("Run 'dotenv-encrypt examples' for usage examples");
        return 0;
    }

    public static int ShowExamples()
    {
        Console.WriteLine("DotEnv Secret Manager - Usage Examples");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("Basic Usage:");
        Console.WriteLine("  dotenv-encrypt encrypt .env");
        Console.WriteLine("  dotenv-encrypt decrypt .env.enc");
        Console.WriteLine("  dotenv-encrypt validate .env.enc");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("Custom Output:");
        Console.WriteLine("  dotenv-encrypt encrypt .env --output .env.production.enc");
        Console.WriteLine("  dotenv-encrypt decrypt .env.enc --output .env.local");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("With Password (not recommended for scripts):");
        Console.WriteLine("  dotenv-encrypt encrypt .env --password mypassword");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("Force Overwrite:");
        Console.WriteLine("  dotenv-encrypt encrypt .env --force");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("File Analysis:");
        Console.WriteLine("  dotenv-encrypt info .env");
        Console.WriteLine("  dotenv-encrypt info .env.enc");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("Typical Workflow:");
        Console.WriteLine("  1. dotenv-encrypt encrypt .env                           # Creates .env.enc");
        Console.WriteLine("  2. git add .env.enc                                      # Safe to commit");
        Console.WriteLine("  3. dotenv-encrypt validate .env.enc                      # Verify integrity");
        Console.WriteLine("  4. dotenv-encrypt decrypt .env.enc --output .env.local   # For development");
        Console.WriteLine();

        ConsoleHelper.WriteInfo("Security Best Practices:");
        Console.WriteLine("  * Add .env, .env.local, *.decrypted to .gitignore");
        Console.WriteLine("  * Only commit .env.enc files");
        Console.WriteLine("  * Use strong, unique passwords");
        Console.WriteLine("  * Store passwords in a secure password manager");
        Console.WriteLine("  * Regularly rotate encryption passwords");

        return 0;
    }
}