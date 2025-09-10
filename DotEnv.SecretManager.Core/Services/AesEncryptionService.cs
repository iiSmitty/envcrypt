using DotEnv.SecretManager.Core.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotEnv.SecretManager.Core.Services
{
    /// <summary>
    /// AES encryption implementation with PBKDF2 key derivation
    /// </summary>
    public class AesEncryptionService : IEncryptionService
    {
        private const int KeySize = 256; // AES-256
        private const int BlockSize = 128; // AES block size
        private const int SaltSize = 16; // 128-bit salt
        private const int IvSize = 16; // 128-bit IV
        private const int Iterations = 10000; // PBKDF2 iterations

        /// <summary>
        /// Encrypts plaintext using AES-256-CBC with PBKDF2 key derivation
        /// Format: [Salt(16)][IV(16)][EncryptedData]
        /// </summary>
        public async Task<string> EncryptAsync(string plaintext, string password)
        {
            if (string.IsNullOrEmpty(plaintext))
                throw new ArgumentException("Plaintext cannot be null or empty", nameof(plaintext));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return await Task.Run(() =>
            {
                var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

                // Generate random salt and IV
                var salt = GenerateRandomBytes(SaltSize);
                var iv = GenerateRandomBytes(IvSize);

                // Derive key from password using PBKDF2
                var key = DeriveKey(password, salt);

                // Encrypt the data
                byte[] encryptedBytes;
                using (var aes = Aes.Create())
                {
                    aes.KeySize = KeySize;
                    aes.BlockSize = BlockSize;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plaintextBytes, 0, plaintextBytes.Length);
                        }
                        encryptedBytes = msEncrypt.ToArray();
                    }
                }

                // Combine salt + IV + encrypted data
                var result = new byte[SaltSize + IvSize + encryptedBytes.Length];
                Array.Copy(salt, 0, result, 0, SaltSize);
                Array.Copy(iv, 0, result, SaltSize, IvSize);
                Array.Copy(encryptedBytes, 0, result, SaltSize + IvSize, encryptedBytes.Length);

                // Return as Base64 string
                return Convert.ToBase64String(result);
            });
        }

        /// <summary>
        /// Decrypts ciphertext using AES-256-CBC
        /// </summary>
        public async Task<string> DecryptAsync(string ciphertext, string password)
        {
            if (string.IsNullOrEmpty(ciphertext))
                throw new ArgumentException("Ciphertext cannot be null or empty", nameof(ciphertext));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return await Task.Run(() =>
            {
                try
                {
                    var ciphertextBytes = Convert.FromBase64String(ciphertext);

                    // Ensure minimum length (salt + IV + at least 1 block)
                    if (ciphertextBytes.Length < SaltSize + IvSize + BlockSize / 8)
                        throw new CryptographicException("Invalid ciphertext format");

                    // Extract salt, IV, and encrypted data
                    var salt = new byte[SaltSize];
                    var iv = new byte[IvSize];
                    var encryptedData = new byte[ciphertextBytes.Length - SaltSize - IvSize];

                    Array.Copy(ciphertextBytes, 0, salt, 0, SaltSize);
                    Array.Copy(ciphertextBytes, SaltSize, iv, 0, IvSize);
                    Array.Copy(ciphertextBytes, SaltSize + IvSize, encryptedData, 0, encryptedData.Length);

                    // Derive key from password and salt
                    var key = DeriveKey(password, salt);

                    // Decrypt the data
                    using (var aes = Aes.Create())
                    {
                        aes.KeySize = KeySize;
                        aes.BlockSize = BlockSize;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.Key = key;
                        aes.IV = iv;

                        using (var decryptor = aes.CreateDecryptor())
                        using (var msDecrypt = new MemoryStream(encryptedData))
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        using (var msPlaintext = new MemoryStream())
                        {
                            csDecrypt.CopyTo(msPlaintext);
                            return Encoding.UTF8.GetString(msPlaintext.ToArray());
                        }
                    }
                }
                catch (Exception ex) when (ex is FormatException || ex is CryptographicException)
                {
                    throw new CryptographicException("Failed to decrypt: invalid password or corrupted data", ex);
                }
            });
        }

        /// <summary>
        /// Validates that ciphertext can be decrypted with the given password
        /// </summary>
        public async Task<bool> ValidateAsync(string ciphertext, string password)
        {
            try
            {
                await DecryptAsync(ciphertext, password);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Derives a key from password and salt using PBKDF2
        /// </summary>
        private static byte[] DeriveKey(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KeySize / 8); // Convert bits to bytes
            }
        }

        /// <summary>
        /// Generates cryptographically secure random bytes
        /// </summary>
        private static byte[] GenerateRandomBytes(int size)
        {
            var bytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }
    }
}