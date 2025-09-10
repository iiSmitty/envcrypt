using DotEnv.SecretManager.Core.Models;
using System.Threading.Tasks;

namespace DotEnv.SecretManager.Core.Interfaces
{
    /// <summary>
    /// Service for encrypting and decrypting data
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a string using the provided password
        /// </summary>
        Task<string> EncryptAsync(string plaintext, string password);

        /// <summary>
        /// Decrypts a string using the provided password
        /// </summary>
        Task<string> DecryptAsync(string ciphertext, string password);

        /// <summary>
        /// Validates that the ciphertext can be decrypted with the password
        /// </summary>
        Task<bool> ValidateAsync(string ciphertext, string password);
    }
}