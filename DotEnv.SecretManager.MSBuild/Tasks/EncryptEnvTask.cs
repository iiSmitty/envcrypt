using Microsoft.Build.Framework;

namespace DotEnv.SecretManager.MSBuild.Tasks;

public class EncryptEnvTask : EnvTaskBase
{
    public string OutputFile { get; set; } = "";

    public override bool Execute()
    {
        try
        {
            LogInfo($"Encrypting: {InputFile}");

            // Set default output file if not provided
            if (string.IsNullOrEmpty(OutputFile))
            {
                OutputFile = $"{InputFile}.enc";
            }

            // Check if input file exists
            if (!System.IO.File.Exists(InputFile))
            {
                LogError($"Input file not found: {InputFile}");
                return false;
            }

            // Check if password is provided
            if (string.IsNullOrEmpty(Password))
            {
                LogError("Password is required for encryption");
                return false;
            }

            // Perform encryption (using GetAwaiter().GetResult() to make it synchronous)
            var result = SecretManager.EncryptFileAsync(InputFile, Password, OutputFile).GetAwaiter().GetResult();

            if (result.Success)
            {
                LogInfo($"Successfully encrypted {result.ProcessedEntries} entries to {OutputFile}");
                return true;
            }
            else
            {
                LogError($"Encryption failed: {result.ErrorMessage}");
                return false;
            }
        }
        catch (Exception ex)
        {
            LogError($"Encryption task failed: {ex.Message}");
            return false;
        }
    }
}