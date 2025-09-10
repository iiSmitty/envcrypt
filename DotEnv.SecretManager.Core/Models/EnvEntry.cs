using System;

namespace DotEnv.SecretManager.Core.Models
{
    /// <summary>
    /// Represents a single key-value pair from an .env file
    /// </summary>
    public class EnvEntry
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public bool IsEncrypted { get; set; }

        public EnvEntry() { }

        public EnvEntry(string key, string value, string? comment = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Comment = comment;
        }
    }
}