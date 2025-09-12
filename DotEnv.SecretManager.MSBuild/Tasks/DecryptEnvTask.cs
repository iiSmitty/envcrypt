using Microsoft.Build.Framework;

namespace DotEnv.SecretManager.MSBuild.Tasks;

public class DecryptEnvTask : EnvTaskBase
{
    public string OutputFile { get; set; } = "";

    public override bool Execute()
    {
        try
        {
            LogInfo($"Decrypting: {InputFile}");

            // Set default output file if not provided  
            if (string.IsNullOrEmpty(OutputFile))
            {
                OutputFile = InputFile.EndsWith(".enc")
                    ? InputFile.Substring(0, InputFile.Length - 4)
                    : $"{InputFile}.decrypted";
            }

            // Check if input file exists
            if (!System.IO.File.Exists(InputFile))
            {
                LogError($"Encrypted file not found: {InputFile}");
                return false;
            }

            // Check if password is provided
            if (string.IsNullOrEmpty(Password))
            {
                LogError("Password is required for decryption");
                return false;
            }

            // Perform decryption
            var result = SecretManager.DecryptFileAsync(InputFile, Password, OutputFile).GetAwaiter().GetResult();

            if (result.Success)
            {
                LogInfo($"Successfully decrypted {result.ProcessedEntries} entries to {OutputFile}");
                return true;
            }
            else
            {
                LogError($"Decryption failed: {result.ErrorMessage}");
                return false;
            }
        }
        catch (Exception ex)
        {
            LogError($"Decryption task failed: {ex.Message}");
            return false;
        }
    }
}