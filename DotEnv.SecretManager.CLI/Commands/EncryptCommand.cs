using DotEnv.SecretManager.CLI.Commands.Options;
using DotEnv.SecretManager.CLI.Infrastructure;
using System.Diagnostics;

namespace DotEnv.SecretManager.CLI.Commands;

public class EncryptCommand : BaseCommand
{
    public EncryptCommand(AppServices services) : base(services) { }

    public override async Task<int> ExecuteAsync(string[] args)
    {
        var options = EncryptOptions.Parse(args);
        if (options == null) return 1;

        try
        {
            if (!CheckFileExists(options.InputFile)) return 1;
            if (!ConfirmOverwrite(options.OutputFile, options.Force)) return 0;

            var password = GetPassword(options.Password, "Enter encryption password: ", true);
            if (string.IsNullOrEmpty(password)) return 1;

            ConsoleHelper.WriteInfo($"Encrypting: {options.InputFile}");
            ConsoleHelper.WriteInfo($"Output to: {options.OutputFile}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();
            ConsoleHelper.WriteProgress("Processing file...");

            var result = await Services.SecretManager.EncryptFileAsync(options.InputFile, password, options.OutputFile);
            stopwatch.Stop();

            if (result.Success)
            {
                ConsoleHelper.WriteSuccess("Encryption completed successfully!");
                Console.WriteLine();

                ConsoleHelper.WriteInfo($"Input file:        {options.InputFile}");
                ConsoleHelper.WriteInfo($"Output file:       {result.OutputFilePath}");
                ConsoleHelper.WriteInfo($"Entries encrypted: {result.ProcessedEntries}");
                ConsoleHelper.WriteInfo($"Duration:          {result.Duration.TotalMilliseconds:F0}ms");
                ConsoleHelper.WriteInfo($"File size:         {ConsoleHelper.GetFileSize(result.OutputFilePath!)}");

                Console.WriteLine();
                WriteSecurityReminder();

                return 0;
            }
            else
            {
                ConsoleHelper.WriteError($"Encryption failed: {result.ErrorMessage}");
                return 1;
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Encryption error: {ex.Message}");
            return 1;
        }
    }

    private static void WriteSecurityReminder()
    {
        ConsoleHelper.WriteInfo("Security Reminders:");
        ConsoleHelper.WriteInfo("  * Store your password safely - it cannot be recovered");
        ConsoleHelper.WriteInfo("  * Encrypted .enc files are safe to commit to git");
        ConsoleHelper.WriteInfo("  * Consider backing up the original file");
    }
}