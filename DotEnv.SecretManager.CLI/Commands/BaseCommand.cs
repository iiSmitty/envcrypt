using DotEnv.SecretManager.CLI.Infrastructure;

namespace DotEnv.SecretManager.CLI.Commands;

public abstract class BaseCommand
{
    protected readonly AppServices Services;

    protected BaseCommand(AppServices services)
    {
        Services = services;
    }

    public abstract Task<int> ExecuteAsync(string[] args);

    protected static string? GetPassword(string? providedPassword, string prompt, bool confirm = false)
    {
        if (!string.IsNullOrEmpty(providedPassword))
        {
            return providedPassword;
        }

        var password = ConsoleHelper.PromptForPassword(prompt);
        if (string.IsNullOrEmpty(password))
        {
            ConsoleHelper.WriteError("Password cannot be empty");
            return null;
        }

        if (confirm)
        {
            var confirmPassword = ConsoleHelper.PromptForPassword("Confirm password: ");
            if (password != confirmPassword)
            {
                ConsoleHelper.WriteError("Passwords do not match");
                return null;
            }
        }

        return password;
    }

    protected static bool CheckFileExists(string filePath)
    {
        if (File.Exists(filePath)) return true;

        ConsoleHelper.WriteError($"File not found: {filePath}");
        ConsoleHelper.WriteSuggestion("Check the file path and try again.");
        return false;
    }

    protected static bool ConfirmOverwrite(string filePath, bool force)
    {
        if (!File.Exists(filePath) || force) return true;

        ConsoleHelper.WriteWarning($"Output file already exists: {filePath}");
        Console.Write("Overwrite? (y/N): ");
        var response = Console.ReadLine()?.ToLower();

        if (response != "y" && response != "yes")
        {
            ConsoleHelper.WriteInfo("Operation cancelled.");
            return false;
        }

        return true;
    }
}