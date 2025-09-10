# DotEnv.SecretManager.Core Usage Examples

## Installation

```bash
dotnet add package DotEnv.SecretManager.Core
```

## Basic Usage

### Simple Encryption/Decryption

```csharp
using DotEnv.SecretManager.Core.Services;

// Setup services
var encryptionService = new AesEncryptionService();
var fileService = new EnvFileService();
var secretManager = new SecretManager(encryptionService, fileService);

// Encrypt a file
var result = await secretManager.EncryptFileAsync(".env", "mypassword");
if (result.Success)
{
    Console.WriteLine($"Encrypted {result.ProcessedEntries} entries to {result.OutputFilePath}");
}

// Decrypt a file
var decryptResult = await secretManager.DecryptFileAsync(".env.enc", "mypassword");
if (decryptResult.Success)
{
    Console.WriteLine($"Decrypted to {decryptResult.OutputFilePath}");
}
```

### Direct String Encryption

```csharp
using DotEnv.SecretManager.Core.Services;

var encryptionService = new AesEncryptionService();

// Encrypt a string
string plaintext = "my-secret-api-key";
string password = "strong-password";
string encrypted = await encryptionService.EncryptAsync(plaintext, password);

// Decrypt it back
string decrypted = await encryptionService.DecryptAsync(encrypted, password);
Console.WriteLine(decrypted); // Output: my-secret-api-key
```

### Working with .env Files

```csharp
using DotEnv.SecretManager.Core.Services;
using DotEnv.SecretManager.Core.Models;

var fileService = new EnvFileService();

// Parse an existing .env file
var entries = await fileService.ParseFileAsync(".env");
foreach (var entry in entries)
{
    if (!string.IsNullOrEmpty(entry.Key))
    {
        Console.WriteLine($"{entry.Key} = {entry.Value}");
    }
}

// Create new entries
var newEntries = new List<EnvEntry>
{
    new EnvEntry("API_KEY", "secret123"),
    new EnvEntry("DB_HOST", "localhost") { Comment = "Development database" }
};

// Write to file
await fileService.WriteFileAsync("output.env", newEntries);
```

### Validation

```csharp
// Check if encrypted file is valid
bool isValid = await secretManager.ValidateFileAsync(".env.enc", "password");
if (isValid)
{
    Console.WriteLine("File can be decrypted successfully");
}
```

## Integration in ASP.NET Core

```csharp
// In Program.cs or Startup.cs
using DotEnv.SecretManager.Core.Services;
using DotEnv.SecretManager.Core.Interfaces;

// Register services
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
builder.Services.AddSingleton<IEnvFileService, EnvFileService>();
builder.Services.AddSingleton<ISecretManager, SecretManager>();

// Use in controllers or services
public class MyController : ControllerBase
{
    private readonly ISecretManager _secretManager;
    
    public MyController(ISecretManager secretManager)
    {
        _secretManager = secretManager;
    }
    
    [HttpPost("encrypt")]
    public async Task<IActionResult> EncryptConfig([FromBody] EncryptRequest request)
    {
        var result = await _secretManager.EncryptFileAsync(
            request.FilePath, 
            request.Password
        );
        
        return result.Success 
            ? Ok(result) 
            : BadRequest(result.ErrorMessage);
    }
}
```

## Security Notes

- Always use strong passwords (consider key derivation from master passwords)
- Store passwords securely (environment variables, key vaults)
- Validate inputs before processing
- Use HTTPS when transmitting encrypted data
- Regular key rotation for production environments