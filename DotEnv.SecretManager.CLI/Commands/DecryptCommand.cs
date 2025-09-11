using DotEnv.SecretManager.CLI.Commands.Options;
using DotEnv.SecretManager.CLI.Infrastructure;
using System.Diagnostics;

namespace DotEnv.SecretManager.CLI.Commands;

public class DecryptCommand : BaseCommand
{
    public DecryptCommand(AppServices services) : base(services) { }

    public override async Task<int> ExecuteAsync(string[] args)
    {
        var options = DecryptOptions.Parse(args);
        if (options == null) return 1;

        try
        {
            if (!CheckFileExists(options.InputFile)) return 1;
            if (!ConfirmOverwrite(options.OutputFile, options.Force)) return 0;

            var password = GetPassword(options.Password, "Enter decryption password: ");
            if (string.IsNullOrEmpty(password)) return 1;

            ConsoleHelper.WriteInfo($"Decrypting: {options.InputFile}");
            ConsoleHelper.WriteInfo($"Output to: {options.OutputFile}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();
            ConsoleHelper.WriteProgress("Processing encrypted file...");

            var result = await Services.SecretManager.DecryptFileAsync(options.InputFile, password, options.OutputFile);
            stopwatch.Stop();

            if (result.Success)
            {
                ConsoleHelper.WriteSuccess("Decryption completed successfully!");
                Console.WriteLine();

                ConsoleHelper.WriteInfo($"Input file:        {options.InputFile}");
                ConsoleHelper.WriteInfo($"Output file:       {result.OutputFilePath}");
                ConsoleHelper.WriteInfo($"Entries decrypted: {result.ProcessedEntries}");
                ConsoleHelper.WriteInfo($"Duration:          {result.Duration.TotalMilliseconds:F0}ms");
                ConsoleHelper.WriteInfo($"File size:         {ConsoleHelper.GetFileSize(result.OutputFilePath!)}");

                Console.WriteLine();
                WriteSecurityWarning();

                return 0;
            }
            else
            {
                ConsoleHelper.WriteError($"Decryption failed: {result.ErrorMessage}");

                if (result.ErrorMessage?.Contains("password") == true)
                {
                    ConsoleHelper.WriteSuggestion("Check your password and try again.");
                    ConsoleHelper.WriteSuggestion("Use 'dotenv-encrypt validate' to test the password.");
                }

                return 1;
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Decryption error: {ex.Message}");
            return 1;
        }
    }

    private static void WriteSecurityWarning()
    {
        ConsoleHelper.WriteWarning("Security Warning:");
        ConsoleHelper.WriteWarning("  * The decrypted file contains plaintext secrets");
        ConsoleHelper.WriteWarning("  * Ensure it's in your .gitignore");
        ConsoleHelper.WriteWarning("  * Delete it when no longer needed");
    }
}