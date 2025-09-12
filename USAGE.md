# DotEnv.SecretManager Usage Examples

This guide provides comprehensive usage examples for all three packages in the DotEnv Secret Manager suite.

## Package Overview

| Package | Best For | Installation |
|---------|----------|-------------|
| **`DotEnv.SecretManager.MSBuild`** | **Automatic team workflows** | `dotnet add package` |
| `DotEnvSecretManager` | Manual CLI operations | `dotnet tool install -g` |
| `DotEnv.SecretManager.Core` | Library integration | `dotnet add package` |

## MSBuild Integration (Recommended)

### Installation

```bash
dotnet add package DotEnv.SecretManager.MSBuild
```

### Basic Automatic Encryption

**1. Enable auto-encryption in your project:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    
    <!-- Enable automatic encryption -->
    <AutoEncryptEnv>true</AutoEncryptEnv>
  </PropertyGroup>
</Project>
```

**2. Set your password and build:**
```bash
# Set password via environment variable
export ENV_ENCRYPTION_PASSWORD=your-secure-password

# Regular build now automatically encrypts .env ? .env.enc
dotnet build
```

**3. Commit only encrypted files:**
```bash
git add .env.enc
git commit -m "Add encrypted environment configuration"
```

### CI/CD Pipeline Integration

**GitHub Actions:**
```yaml
name: Deploy Application

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Build with automatic decryption
      run: dotnet build -p:AutoDecryptEnv=true
      env:
        ENV_ENCRYPTION_PASSWORD: ${{ secrets.ENV_DECRYPTION_PASSWORD }}
    
    - name: Deploy
      run: dotnet publish -c Release
```

**Azure DevOps:**
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Build with encrypted environment'
  inputs:
    command: 'build'
    arguments: '-p:AutoDecryptEnv=true'
  env:
    ENV_ENCRYPTION_PASSWORD: $(EnvDecryptionPassword)
```

### Multi-Environment Setup

**Project configuration for multiple environments:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <AutoEncryptEnv>true</AutoEncryptEnv>
  </PropertyGroup>

  <!-- Development environment -->
  <PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <EnvInputFile>$(ProjectDir).env.development</EnvInputFile>
    <EnvOutputFile>$(ProjectDir).env.development.enc</EnvOutputFile>
  </PropertyGroup>

  <!-- Production environment -->
  <PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <EnvInputFile>$(ProjectDir).env.production</EnvInputFile>
    <EnvOutputFile>$(ProjectDir).env.production.enc</EnvOutputFile>
  </PropertyGroup>
</Project>
```

**Usage:**
```bash
# Development build - encrypts .env.development
export ENV_ENCRYPTION_PASSWORD=dev-password
dotnet build -c Debug

# Production build - encrypts .env.production  
export ENV_ENCRYPTION_PASSWORD=prod-password
dotnet build -c Release
```

### Advanced MSBuild Configuration

**Custom file paths and conditional logic:**
```xml
<PropertyGroup>
  <!-- Custom file locations -->
  <EnvInputFile>$(ProjectDir)config/secrets.env</EnvInputFile>
  <EnvOutputFile>$(ProjectDir)config/secrets.env.encrypted</EnvOutputFile>
  
  <!-- Only auto-encrypt in Release builds -->
  <AutoEncryptEnv Condition="'$(Configuration)' == 'Release'">true</AutoEncryptEnv>
  
  <!-- Auto-decrypt in CI environments -->
  <AutoDecryptEnv Condition="'$(CI)' == 'true'">true</AutoDecryptEnv>
  
  <!-- Use different password sources -->
  <EnvEncryptionPassword Condition="'$(EnvEncryptionPassword)' == ''">$(CUSTOM_PASSWORD_VAR)</EnvEncryptionPassword>
</PropertyGroup>

<!-- Manual targets for advanced scenarios -->
<Target Name="EncryptAllEnvironments">
  <MSBuild Projects="$(MSBuildProjectFile)" 
           Targets="EncryptEnvFiles" 
           Properties="EnvInputFile=.env.development;EnvOutputFile=.env.development.enc;EnvEncryptionPassword=$(DEV_PASSWORD)" />
  <MSBuild Projects="$(MSBuildProjectFile)" 
           Targets="EncryptEnvFiles" 
           Properties="EnvInputFile=.env.production;EnvOutputFile=.env.production.enc;EnvEncryptionPassword=$(PROD_PASSWORD)" />
</Target>
```

### Team Workflow Example

**New team member onboarding:**
```bash
# Day 1 - New developer joins
git clone https://company-repo/awesome-app
cd awesome-app

# Get password from team lead (secure channel)
export ENV_ENCRYPTION_PASSWORD=team-shared-password

# Build automatically decrypts configuration
dotnet build
dotnet run  # App starts with correct environment
```

**Daily development workflow:**
```bash
# Developer updates environment variables
vim .env.development

# Regular build automatically encrypts changes
dotnet build

# Commit encrypted file
git add .env.development.enc
git commit -m "Update API endpoints for new service"
git push
```

## CLI Tool Usage

### Installation

```bash
dotnet tool install -g DotEnvSecretManager
```

### Basic Operations

**Encrypt a file:**
```bash
# Basic encryption (prompts for password)
dotenv-encrypt encrypt .env

# With custom output
dotenv-encrypt encrypt .env --output .env.production.enc

# With password (not recommended for scripts)
dotenv-encrypt encrypt .env --password mypassword

# Force overwrite existing files
dotenv-encrypt encrypt .env --force
```

**Decrypt a file:**
```bash
# Basic decryption
dotenv-encrypt decrypt .env.enc

# Custom output location
dotenv-encrypt decrypt .env.enc --output .env.local

# Force overwrite
dotenv-encrypt decrypt .env.enc --force
```

**Validate encrypted files:**
```bash
# Test if file can be decrypted (password prompt)
dotenv-encrypt validate .env.enc

# Quick validation in scripts
echo "password123" | dotenv-encrypt validate .env.enc --password-stdin
```

### CLI Workflow Examples

**Individual developer workflow:**
```bash
# 1. Create/edit environment file
echo "API_KEY=sk_live_abc123" > .env
echo "DB_HOST=localhost" >> .env
echo "DB_PASSWORD=secret123" >> .env

# 2. Encrypt for storage
dotenv-encrypt encrypt .env
# Enter password when prompted

# 3. Verify encryption worked
dotenv-encrypt validate .env.enc

# 4. Safe to commit encrypted file
git add .env.enc
git commit -m "Add environment configuration"

# 5. For deployment, decrypt with different output
dotenv-encrypt decrypt .env.enc --output .env.production
```

**Script automation:**
```bash
#!/bin/bash
# deploy.sh - deployment script

set -e

# Decrypt environment for deployment
dotenv-encrypt decrypt .env.production.enc --output .env --password "$DEPLOY_PASSWORD"

# Deploy application
dotnet publish -c Release

# Clean up decrypted file
rm .env

echo "Deployment completed successfully"
```

## Library Integration (Programmatic)

### Installation

```bash
dotnet add package DotEnv.SecretManager.Core
```

### Basic Usage

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
else
{
    Console.WriteLine($"Encryption failed: {result.ErrorMessage}");
}
```

### Direct String Encryption

```csharp
var encryptionService = new AesEncryptionService();

// Encrypt individual values
string plaintext = "my-secret-api-key";
string password = "strong-password";
string encrypted = await encryptionService.EncryptAsync(plaintext, password);

// Decrypt back
string decrypted = await encryptionService.DecryptAsync(encrypted, password);
Console.WriteLine(decrypted); // Output: my-secret-api-key

// Validate encrypted value
bool isValid = await encryptionService.ValidateAsync(encrypted, password);
Console.WriteLine($"Valid: {isValid}"); // Output: Valid: True
```

### Working with .env Files

```csharp
using DotEnv.SecretManager.Core.Models;

var fileService = new EnvFileService();

// Parse an existing .env file
var entries = await fileService.ParseFileAsync(".env");
foreach (var entry in entries)
{
    if (!string.IsNullOrEmpty(entry.Key))
    {
        Console.WriteLine($"{entry.Key} = {entry.Value}");
        if (!string.IsNullOrEmpty(entry.Comment))
        {
            Console.WriteLine($"  # {entry.Comment}");
        }
    }
}

// Create new entries programmatically
var newEntries = new List<EnvEntry>
{
    new EnvEntry("", "") { Comment = "Application Configuration" },
    new EnvEntry("API_KEY", "secret123") { Comment = "Third-party API key" },
    new EnvEntry("DB_HOST", "localhost"),
    new EnvEntry("DB_PORT", "5432"),
    new EnvEntry("FEATURE_FLAGS", "dark_mode=true,new_ui=false") { Comment = "Feature toggles" }
};

// Write to file
await fileService.WriteFileAsync("output.env", newEntries);
```

### ASP.NET Core Integration

```csharp
// Program.cs
using DotEnv.SecretManager.Core.Services;
using DotEnv.SecretManager.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Register encryption services
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
builder.Services.AddSingleton<IEnvFileService, EnvFileService>();
builder.Services.AddSingleton<ISecretManager, SecretManager>();

// If you need to decrypt at startup
var secretManager = new SecretManager(
    new AesEncryptionService(), 
    new EnvFileService());

if (File.Exists(".env.enc"))
{
    var password = Environment.GetEnvironmentVariable("ENV_ENCRYPTION_PASSWORD");
    if (!string.IsNullOrEmpty(password))
    {
        var result = await secretManager.DecryptFileAsync(".env.enc", password, ".env.temp");
        if (result.Success)
        {
            // Load decrypted environment variables
            DotNetEnv.Env.Load(".env.temp");
            File.Delete(".env.temp"); // Clean up
        }
    }
}

var app = builder.Build();

// Use in controllers
[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ISecretManager _secretManager;
    
    public ConfigController(ISecretManager secretManager)
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
            ? Ok(new { message = $"Encrypted {result.ProcessedEntries} entries", result.OutputFilePath })
            : BadRequest(new { error = result.ErrorMessage });
    }
    
    [HttpPost("decrypt")]
    public async Task<IActionResult> DecryptConfig([FromBody] DecryptRequest request)
    {
        var result = await _secretManager.DecryptFileAsync(
            request.FilePath,
            request.Password,
            request.OutputPath
        );
        
        return result.Success 
            ? Ok(new { message = $"Decrypted {result.ProcessedEntries} entries" })
            : BadRequest(new { error = result.ErrorMessage });
    }
}
```

### Custom Configuration Management

```csharp
public class EnvironmentManager
{
    private readonly ISecretManager _secretManager;
    private readonly ILogger<EnvironmentManager> _logger;
    
    public EnvironmentManager(ISecretManager secretManager, ILogger<EnvironmentManager> logger)
    {
        _secretManager = secretManager;
        _logger = logger;
    }
    
    public async Task<bool> LoadEnvironmentAsync(string environment, string password)
    {
        var encryptedFile = $".env.{environment}.enc";
        var decryptedFile = $".env.{environment}.temp";
        
        try
        {
            // Decrypt environment file
            var result = await _secretManager.DecryptFileAsync(encryptedFile, password, decryptedFile);
            
            if (!result.Success)
            {
                _logger.LogError("Failed to decrypt environment {Environment}: {Error}", 
                    environment, result.ErrorMessage);
                return false;
            }
            
            // Load into current process
            DotNetEnv.Env.Load(decryptedFile);
            
            // Clean up temporary file
            File.Delete(decryptedFile);
            
            _logger.LogInformation("Successfully loaded environment {Environment} with {Count} variables", 
                environment, result.ProcessedEntries);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading environment {Environment}", environment);
            
            // Ensure cleanup
            if (File.Exists(decryptedFile))
                File.Delete(decryptedFile);
                
            return false;
        }
    }
    
    public async Task<bool> UpdateEnvironmentAsync(string environment, Dictionary<string, string> variables, string password)
    {
        var envFile = $".env.{environment}";
        
        try
        {
            // Create env entries
            var entries = variables.Select(kv => new EnvEntry(kv.Key, kv.Value)).ToList();
            
            // Write temporary file
            await _secretManager.EnvFileService.WriteFileAsync(envFile, entries);
            
            // Encrypt the file
            var result = await _secretManager.EncryptFileAsync(envFile, password);
            
            // Clean up temporary file
            File.Delete(envFile);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully updated environment {Environment}", environment);
                return true;
            }
            else
            {
                _logger.LogError("Failed to encrypt environment {Environment}: {Error}", 
                    environment, result.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating environment {Environment}", environment);
            return false;
        }
    }
}
```

## Security Best Practices

### Password Management
- **Never hardcode passwords** in source code
- Use **environment variables** for CI/CD passwords
- Store passwords in **secure vaults** (Azure Key Vault, AWS Secrets Manager)
- **Rotate passwords** regularly for production environments
- Use **different passwords** for different environments

### File Handling
- Add `.env*` to `.gitignore` (except `.enc` files)
- **Delete temporary decrypted files** after use
- Use **secure file permissions** on servers (600 or 644)
- **Backup encrypted files** before making changes

### Team Security
- Share passwords via **secure channels** (password managers, encrypted communication)
- **Document** who has access to which environment passwords
- **Audit** encrypted file changes through git history
- **Train** team members on secure practices

### CI/CD Security
- Use **CI/CD secret management** (GitHub Secrets, Azure DevOps Variables)
- **Limit** who can access production passwords
- **Clean up** decrypted files in CI/CD pipelines
- **Monitor** for accidental secret commits

## Troubleshooting

### Common Issues

**MSBuild integration not working:**
```bash
# Check if package is properly referenced
dotnet list package | grep DotEnv.SecretManager.MSBuild

# Verify environment variable is set
echo $ENV_ENCRYPTION_PASSWORD

# Test manual trigger
dotnet build -t:EncryptEnvFiles -p:AutoEncryptEnv=true
```

**CLI tool not found:**
```bash
# Reinstall global tool
dotnet tool uninstall -g DotEnvSecretManager
dotnet tool install -g DotEnvSecretManager

# Check PATH
dotnet tool list -g
```

**Decryption fails:**
- Verify password is correct
- Check if file is corrupted (try with another encrypted file)
- Ensure file was encrypted with same tool version

**Build fails with MSBuild integration:**
- Check project file syntax
- Verify package version compatibility
- Remove and re-add package reference