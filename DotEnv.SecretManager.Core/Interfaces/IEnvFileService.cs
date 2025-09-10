using DotEnv.SecretManager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotEnv.SecretManager.Core.Interfaces
{
    /// <summary>
    /// Service for handling .env file operations
    /// </summary>
    public interface IEnvFileService
    {
        /// <summary>
        /// Parses a .env file and returns the entries
        /// </summary>
        Task<List<EnvEntry>> ParseFileAsync(string filePath);

        /// <summary>
        /// Writes env entries to a file
        /// </summary>
        Task WriteFileAsync(string filePath, List<EnvEntry> entries);

        /// <summary>
        /// Checks if a file exists and is readable
        /// </summary>
        Task<bool> FileExistsAsync(string filePath);

        /// <summary>
        /// Creates a backup of the original file
        /// </summary>
        Task<string> CreateBackupAsync(string filePath);
    }
}