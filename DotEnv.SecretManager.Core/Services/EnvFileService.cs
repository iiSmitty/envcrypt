using DotEnv.SecretManager.Core.Interfaces;
using DotEnv.SecretManager.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotEnv.SecretManager.Core.Services
{
    /// <summary>
    /// Service for parsing and writing .env files
    /// </summary>
    public class EnvFileService : IEnvFileService
    {
        // Regex pattern for parsing .env entries
        // Matches: KEY=value, KEY="value", KEY='value', with optional comments
        private static readonly Regex EnvLineRegex = new Regex(
            @"^(?<key>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<value>""[^""]*""|'[^']*'|[^#\r\n]*?)(?:\s*#(?<comment>.*))?$",
            RegexOptions.Compiled | RegexOptions.Multiline
        );

        private static readonly Regex CommentOnlyRegex = new Regex(
            @"^\s*#(?<comment>.*)$",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Parses a .env file and returns the entries
        /// </summary>
        public async Task<List<EnvEntry>> ParseFileAsync(string filePath)
        {
            if (!await FileExistsAsync(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var entries = new List<EnvEntry>();
            var lines = await File.ReadAllLinesAsync(filePath);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Handle comment-only lines
                var commentMatch = CommentOnlyRegex.Match(trimmedLine);
                if (commentMatch.Success)
                {
                    entries.Add(new EnvEntry("", "")
                    {
                        Comment = commentMatch.Groups["comment"].Value.Trim()
                    });
                    continue;
                }

                // Parse key-value pairs
                var match = EnvLineRegex.Match(trimmedLine);
                if (match.Success)
                {
                    var key = match.Groups["key"].Value;
                    var value = match.Groups["value"].Value.Trim();
                    var comment = match.Groups["comment"].Success ?
                        match.Groups["comment"].Value.Trim() : null;

                    // Remove quotes from quoted values
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    // Check if value appears to be encrypted (base64-like pattern)
                    var isEncrypted = IsLikelyEncrypted(value);

                    entries.Add(new EnvEntry(key, value, comment)
                    {
                        IsEncrypted = isEncrypted
                    });
                }
                else if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    // Malformed line - add as comment for preservation
                    entries.Add(new EnvEntry("", "")
                    {
                        Comment = $"MALFORMED: {trimmedLine}"
                    });
                }
            }

            return entries;
        }

        /// <summary>
        /// Writes env entries to a file
        /// </summary>
        public async Task WriteFileAsync(string filePath, List<EnvEntry> entries)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            var lines = new List<string>();

            foreach (var entry in entries)
            {
                // Handle comment-only entries
                if (string.IsNullOrEmpty(entry.Key) && string.IsNullOrEmpty(entry.Value))
                {
                    if (!string.IsNullOrEmpty(entry.Comment))
                    {
                        lines.Add($"# {entry.Comment}");
                    }
                    continue;
                }

                // Format key-value pair
                var line = new StringBuilder();
                line.Append(entry.Key);
                line.Append('=');

                // Quote value if it contains spaces or special characters
                var value = entry.Value;
                if (ShouldQuoteValue(value))
                {
                    line.Append('"');
                    line.Append(value.Replace("\"", "\\\""));
                    line.Append('"');
                }
                else
                {
                    line.Append(value);
                }

                // Add comment if present
                if (!string.IsNullOrEmpty(entry.Comment))
                {
                    line.Append(" # ");
                    line.Append(entry.Comment);
                }

                lines.Add(line.ToString());
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllLinesAsync(filePath, lines, Encoding.UTF8);
        }

        /// <summary>
        /// Checks if a file exists and is readable
        /// </summary>
        public async Task<bool> FileExistsAsync(string filePath)
        {
            return await Task.Run(() => File.Exists(filePath));
        }

        /// <summary>
        /// Creates a backup of the original file
        /// </summary>
        public async Task<string> CreateBackupAsync(string filePath)
        {
            if (!await FileExistsAsync(filePath))
                throw new FileNotFoundException($"Cannot backup non-existent file: {filePath}");

            var backupPath = $"{filePath}.backup.{DateTime.UtcNow:yyyyMMddHHmmss}";
            await Task.Run(() => File.Copy(filePath, backupPath));

            return backupPath;
        }

        /// <summary>
        /// Determines if a value should be quoted in the .env file
        /// </summary>
        private static bool ShouldQuoteValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // Quote if contains spaces, quotes, or starts/ends with whitespace
            return value.Contains(' ') ||
                   value.Contains('\t') ||
                   value.Contains('"') ||
                   value.Contains('\'') ||
                   value.Contains('#') ||
                   value.StartsWith(" ") ||
                   value.EndsWith(" ");
        }

        /// <summary>
        /// Simple heuristic to detect if a value looks encrypted (base64-like)
        /// </summary>
        private static bool IsLikelyEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 20)
                return false;

            // Check if it's a valid base64 string of reasonable length
            try
            {
                var bytes = Convert.FromBase64String(value);

                // Encrypted values should be at least 32 bytes (salt + IV + some data)
                return bytes.Length >= 32 &&
                       value.All(c => char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '=');
            }
            catch
            {
                return false;
            }
        }
    }
}