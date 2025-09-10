using DotEnv.SecretManager.Core.Interfaces;
using DotEnv.SecretManager.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotEnv.SecretManager.Core.Services
{
    /// <summary>
    /// Main service that orchestrates encryption/decryption of .env files
    /// </summary>
    public class SecretManager : ISecretManager
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IEnvFileService _envFileService;

        public SecretManager(IEncryptionService encryptionService, IEnvFileService envFileService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _envFileService = envFileService ?? throw new ArgumentNullException(nameof(envFileService));
        }

        /// <summary>
        /// Encrypts a .env file
        /// </summary>
        public async Task<EncryptionResult> EncryptFileAsync(string inputFilePath, string password, string? outputFilePath = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(inputFilePath))
                    return EncryptionResult.CreateFailure("Input file path cannot be empty");

                if (string.IsNullOrEmpty(password))
                    return EncryptionResult.CreateFailure("Password cannot be empty");

                if (!await _envFileService.FileExistsAsync(inputFilePath))
                    return EncryptionResult.CreateFailure($"Input file not found: {inputFilePath}");

                // Determine output file path
                outputFilePath ??= GetDefaultEncryptedFileName(inputFilePath);

                // Parse the input file
                var entries = await _envFileService.ParseFileAsync(inputFilePath);

                if (!entries.Any())
                    return EncryptionResult.CreateFailure("Input file contains no valid entries");

                // Encrypt all non-empty values
                var processedEntries = 0;
                foreach (var entry in entries.Where(e => !string.IsNullOrEmpty(e.Key) && !string.IsNullOrEmpty(e.Value)))
                {
                    if (!entry.IsEncrypted) // Skip already encrypted values
                    {
                        entry.Value = await _encryptionService.EncryptAsync(entry.Value, password);
                        entry.IsEncrypted = true;
                        processedEntries++;
                    }
                }

                // Add encryption header comment
                var headerComment = $"Encrypted with DotEnv Secret Manager on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
                entries.Insert(0, new EnvEntry("", "")
                {
                    Comment = headerComment
                });

                // Write the encrypted file
                await _envFileService.WriteFileAsync(outputFilePath, entries);

                stopwatch.Stop();
                return EncryptionResult.CreateSuccess(outputFilePath, processedEntries, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return EncryptionResult.CreateFailure($"Encryption failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Decrypts an encrypted .env file
        /// </summary>
        public async Task<DecryptionResult> DecryptFileAsync(string inputFilePath, string password, string? outputFilePath = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(inputFilePath))
                    return DecryptionResult.CreateFailure("Input file path cannot be empty");

                if (string.IsNullOrEmpty(password))
                    return DecryptionResult.CreateFailure("Password cannot be empty");

                if (!await _envFileService.FileExistsAsync(inputFilePath))
                    return DecryptionResult.CreateFailure($"Input file not found: {inputFilePath}");

                // Determine output file path
                outputFilePath ??= GetDefaultDecryptedFileName(inputFilePath);

                // Parse the encrypted file
                var entries = await _envFileService.ParseFileAsync(inputFilePath);

                if (!entries.Any())
                    return DecryptionResult.CreateFailure("Input file contains no valid entries");

                // Decrypt all encrypted values
                var processedEntries = 0;
                foreach (var entry in entries.Where(e => !string.IsNullOrEmpty(e.Key) && !string.IsNullOrEmpty(e.Value)))
                {
                    if (entry.IsEncrypted || IsLikelyEncrypted(entry.Value))
                    {
                        try
                        {
                            entry.Value = await _encryptionService.DecryptAsync(entry.Value, password);
                            entry.IsEncrypted = false;
                            processedEntries++;
                        }
                        catch (Exception ex)
                        {
                            return DecryptionResult.CreateFailure($"Failed to decrypt '{entry.Key}': {ex.Message}");
                        }
                    }
                }

                // Remove encryption header if present
                var headerEntry = entries.FirstOrDefault(e =>
                    string.IsNullOrEmpty(e.Key) &&
                    !string.IsNullOrEmpty(e.Comment) &&
                    e.Comment.Contains("Encrypted with DotEnv Secret Manager"));

                if (headerEntry != null)
                {
                    entries.Remove(headerEntry);
                }

                // Write the decrypted file
                await _envFileService.WriteFileAsync(outputFilePath, entries);

                stopwatch.Stop();
                return DecryptionResult.CreateSuccess(outputFilePath, processedEntries, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return DecryptionResult.CreateFailure($"Decryption failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates an encrypted file can be decrypted
        /// </summary>
        public async Task<bool> ValidateFileAsync(string encryptedFilePath, string password)
        {
            try
            {
                if (!await _envFileService.FileExistsAsync(encryptedFilePath))
                    return false;

                var entries = await _envFileService.ParseFileAsync(encryptedFilePath);

                // Try to decrypt at least one encrypted value
                var encryptedEntry = entries.FirstOrDefault(e =>
                    !string.IsNullOrEmpty(e.Key) &&
                    !string.IsNullOrEmpty(e.Value) &&
                    (e.IsEncrypted || IsLikelyEncrypted(e.Value)));

                if (encryptedEntry == null)
                    return false; // No encrypted entries found

                return await _encryptionService.ValidateAsync(encryptedEntry.Value, password);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the default encrypted filename (.env -> .env.enc)
        /// </summary>
        private static string GetDefaultEncryptedFileName(string inputFilePath)
        {
            return $"{inputFilePath}.enc";
        }

        /// <summary>
        /// Gets the default decrypted filename (.env.enc -> .env)
        /// </summary>
        private static string GetDefaultDecryptedFileName(string inputFilePath)
        {
            if (inputFilePath.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
            {
                return inputFilePath.Substring(0, inputFilePath.Length - 4);
            }

            // If it doesn't end with .enc, add .decrypted
            return $"{inputFilePath}.decrypted";
        }

        /// <summary>
        /// Simple heuristic to detect if a value looks encrypted
        /// </summary>
        private static bool IsLikelyEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 20)
                return false;

            try
            {
                var bytes = Convert.FromBase64String(value);
                return bytes.Length >= 32; // At least salt + IV + some data
            }
            catch
            {
                return false;
            }
        }
    }
}