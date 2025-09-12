using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using DotEnv.SecretManager.Core.Interfaces;
using DotEnv.SecretManager.Core.Services;

namespace DotEnv.SecretManager.MSBuild.Tasks;

public abstract class EnvTaskBase : Microsoft.Build.Utilities.Task 
{
    [Required]
    public string InputFile { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    protected ISecretManager SecretManager { get; }

    protected EnvTaskBase()
    {
        var encryptionService = new AesEncryptionService();
        var fileService = new EnvFileService();
        SecretManager = new Core.Services.SecretManager(encryptionService, fileService);
    }

    protected void LogInfo(string message)
    {
        Log.LogMessage(MessageImportance.Normal, $"[DotEnv] {message}");
    }

    protected void LogWarning(string message)
    {
        Log.LogWarning($"[DotEnv] {message}");
    }

    protected void LogError(string message)
    {
        Log.LogError($"[DotEnv] {message}");
    }
    public abstract override bool Execute();
}