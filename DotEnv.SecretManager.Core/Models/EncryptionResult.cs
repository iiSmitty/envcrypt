using System;

namespace DotEnv.SecretManager.Core.Models
{
    /// <summary>
    /// Result of an encryption operation
    /// </summary>
    public class EncryptionResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? OutputFilePath { get; set; }
        public int ProcessedEntries { get; set; }
        public TimeSpan Duration { get; set; }

        public static EncryptionResult CreateSuccess(string outputPath, int entries, TimeSpan duration)
        {
            return new EncryptionResult
            {
                Success = true,
                OutputFilePath = outputPath,
                ProcessedEntries = entries,
                Duration = duration
            };
        }

        public static EncryptionResult CreateFailure(string error)
        {
            return new EncryptionResult
            {
                Success = false,
                ErrorMessage = error
            };
        }
    }
}