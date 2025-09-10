using DotEnv.SecretManager.Core.Models;
using System.Threading.Tasks;

namespace DotEnv.SecretManager.Core.Interfaces
{
    /// <summary>
    /// Main service that orchestrates encryption/decryption of .env files
    /// </summary>
    public interface ISecretManager
    {
        /// <summary>
        /// Encrypts a .env file
        /// </summary>
        Task<EncryptionResult> EncryptFileAsync(string inputFilePath, string password, string? outputFilePath = null);

        /// <summary>
        /// Decrypts an encrypted .env file
        /// </summary>
        Task<DecryptionResult> DecryptFileAsync(string inputFilePath, string password, string? outputFilePath = null);

        /// <summary>
        /// Validates an encrypted file can be decrypted
        /// </summary>
        Task<bool> ValidateFileAsync(string encryptedFilePath, string password);
    }
}