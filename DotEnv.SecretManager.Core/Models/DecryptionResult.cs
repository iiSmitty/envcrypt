using System;

namespace DotEnv.SecretManager.Core.Models
{
    /// <summary>
    /// Result of a decryption operation
    /// </summary>
    public class DecryptionResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? OutputFilePath { get; set; }
        public int ProcessedEntries { get; set; }
        public TimeSpan Duration { get; set; }

        public static DecryptionResult CreateSuccess(string outputPath, int entries, TimeSpan duration)
        {
            return new DecryptionResult
            {
                Success = true,
                OutputFilePath = outputPath,
                ProcessedEntries = entries,
                Duration = duration
            };
        }

        public static DecryptionResult CreateFailure(string error)
        {
            return new DecryptionResult
            {
                Success = false,
                ErrorMessage = error
            };
        }
    }
}