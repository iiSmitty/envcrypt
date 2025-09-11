using DotEnv.SecretManager.CLI.Commands.Options;
using DotEnv.SecretManager.CLI.Infrastructure;

namespace DotEnv.SecretManager.CLI.Commands;

public class ValidateCommand : BaseCommand
{
    public ValidateCommand(AppServices services) : base(services) { }

    public override async Task<int> ExecuteAsync(string[] args)
    {
        var options = ValidateOptions.Parse(args);
        if (options == null) return 1;

        try
        {
            if (!CheckFileExists(options.InputFile)) return 1;

            var password = GetPassword(options.Password, "Enter password to validate: ");
            if (string.IsNullOrEmpty(password)) return 1;

            ConsoleHelper.WriteInfo($"Validating: {options.InputFile}");
            ConsoleHelper.WriteProgress("Checking file format and password...");

            var isValid = await Services.SecretManager.ValidateFileAsync(options.InputFile, password);

            if (isValid)
            {
                ConsoleHelper.WriteSuccess("Validation successful!");
                ConsoleHelper.WriteInfo("The file can be decrypted with the provided password.");

                var fileInfo = new FileInfo(options.InputFile);
                ConsoleHelper.WriteInfo($"File size: {ConsoleHelper.GetFileSize(options.InputFile)}");
                ConsoleHelper.WriteInfo($"Modified:  {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");

                return 0;
            }
            else
            {
                ConsoleHelper.WriteError("Validation failed!");
                ConsoleHelper.WriteError("The file cannot be decrypted with the provided password.");
                ConsoleHelper.WriteSuggestion("Check your password and file format.");
                return 1;
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Validation error: {ex.Message}");
            return 1;
        }
    }
}