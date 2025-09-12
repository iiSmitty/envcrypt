# DotEnv Secret Manager

A secure .env file encryption tool for C#/.NET developers with automatic MSBuild integration.

## What This Solves

- **Accidental commits**: Encrypt your .env files so secrets never leak into git
- **Team sharing**: Safely share encrypted configuration with your team
- **Environment security**: Keep secrets encrypted at rest
- **Build automation**: Automatic encryption/decryption during builds

## Installation & Setup

### Option 1: MSBuild Integration (Recommended - Automatic)
Perfect for teams and automatic workflows:

```bash
# Add MSBuild package to your project
dotnet add package DotEnv.SecretManager.MSBuild
```

**Enable automatic encryption in your project file:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    
    <!-- Enable automatic .env encryption during builds -->
    <AutoEncryptEnv>true</AutoEncryptEnv>
  </PropertyGroup>
</Project>
```

**Set your encryption password:**
```bash
# Via environment variable (recommended)
export ENV_ENCRYPTION_PASSWORD=your-secure-password

# Or via MSBuild property
dotnet build -p:EnvEncryptionPassword=your-secure-password
```

### Option 2: Global CLI Tool (Manual Use)
For individual developers who prefer manual control:

```bash
# Install the global tool
dotnet tool install -g --add-source ./bin/Release DotEnvSecretManager

# Use anywhere
dotenv-encrypt encrypt .env
```

### Option 3: Library Integration
For programmatic use in your applications:

```bash
# Add to your project
dotnet add package DotEnv.SecretManager.Core
```

### Option 4: Build from Source
```bash
git clone https://github.com/iiSmitty/envcrypt
cd DotEnvSecretManager
dotnet build
dotnet run --project DotEnv.SecretManager.CLI encrypt .env
```

## MSBuild Integration (Automatic Workflow)

Transform your development workflow with zero-effort encryption:

### The New Developer Experience

**Before (Manual):**
```bash
vim .env                           # Edit secrets
dotenv-encrypt encrypt .env        # Remember to encrypt
git add .env.enc                   # Manual process
git commit -m "Update config"
```

**After (Automatic):**
```bash
vim .env                           # Edit secrets
dotnet build                       # Encryption happens automatically
git add .env.enc                   # Only encrypted files exist
git commit -m "Update config"      # Zero extra steps
```

### Team Onboarding

**New team member setup:**
```bash
git clone company-repo
cd awesome-app
dotnet build    # Password from environment variable, auto-decrypts
dotnet run      # App starts with correct configuration
```

### CI/CD Integration

**GitHub Actions:**
```yaml
- name: Build and Deploy
  run: dotnet build -p:EnvEncryptionPassword=${{ secrets.ENV_PASSWORD }}
  env:
    ENV_ENCRYPTION_PASSWORD: ${{ secrets.ENV_PASSWORD }}
# Automatic decryption during build, deploys with correct config
```

**Azure DevOps:**
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    arguments: '-p:EnvEncryptionPassword=$(EnvironmentPassword)'
```

### Multi-Environment Configuration

**Environment-specific encryption:**
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Development'">
  <EnvInputFile>.env.development</EnvInputFile>
  <EnvOutputFile>.env.development.enc</EnvOutputFile>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'Production'">
  <EnvInputFile>.env.production</EnvInputFile>
  <EnvOutputFile>.env.production.enc</EnvOutputFile>
</PropertyGroup>
```

**Build for specific environments:**
```bash
dotnet build -c Development  # Uses .env.development ? .env.development.enc
dotnet build -c Production   # Uses .env.production ? .env.production.enc
```

### Advanced MSBuild Configuration

```xml
<PropertyGroup>
  <!-- Custom file paths -->
  <EnvInputFile>$(ProjectDir)config/.env</EnvInputFile>
  <EnvOutputFile>$(ProjectDir)config/.env.encrypted</EnvOutputFile>
  
  <!-- Conditional automation -->
  <AutoEncryptEnv Condition="'$(Configuration)' == 'Release'">true</AutoEncryptEnv>
  <AutoDecryptEnv Condition="'$(CI)' == 'true'">true</AutoDecryptEnv>
  
  <!-- Password from different sources -->
  <EnvEncryptionPassword Condition="'$(EnvEncryptionPassword)' == ''">$(ENV_ENCRYPTION_PASSWORD)</EnvEncryptionPassword>
</PropertyGroup>
```

## CLI Commands (Manual Tool)

### Core Commands

#### `encrypt` - Encrypt a .env file
```bash
# Basic encryption (prompts for password)
dotenv-encrypt encrypt .env

# Custom output file
dotenv-encrypt encrypt .env --output .env.production.enc

# Force overwrite existing files
dotenv-encrypt encrypt .env --force
```

#### `decrypt` - Decrypt an encrypted file
```bash
# Basic decryption (prompts for password)
dotenv-encrypt decrypt .env.enc

# Custom output file
dotenv-encrypt decrypt .env.enc --output .env.local

# Force overwrite
dotenv-encrypt decrypt .env.enc --force
```

#### `validate` - Test if file can be decrypted
```bash
# Validate with password prompt
dotenv-encrypt validate .env.enc
```

#### `help` - Show usage information
```bash
dotenv-encrypt help           # General help
```

## Quick Start Examples

### 1. MSBuild Integration Setup (5 minutes)
```bash
# Add package to your project
dotnet add package DotEnv.SecretManager.MSBuild

# Edit your .csproj to enable auto-encryption
echo '<AutoEncryptEnv>true</AutoEncryptEnv>' # Add to PropertyGroup

# Create test .env file
echo "API_KEY=sk_test_123456789" > .env
echo "DB_PASSWORD=super_secret_password" >> .env

# Set password and build
export ENV_ENCRYPTION_PASSWORD=testpassword123
dotnet build

# Check results - .env.enc should be created automatically
ls -la .env*
```

### 2. Manual CLI Usage
```bash
# Install CLI tool
dotenv tool install -g DotEnvSecretManager

# Create sample .env file
echo "API_KEY=test-secret-key-123" > .env
echo "DB_PASSWORD=super-secret-password" >> .env

# Encrypt the file
dotenv-encrypt encrypt .env

# Verify it worked
dotenv-encrypt validate .env.enc

# Decrypt when needed
dotenv-encrypt decrypt .env.enc --output .env.local
```

### 3. Team Workflow
```bash
# Developer A: Encrypt and commit
dotenv-encrypt encrypt .env
git add .env.enc
git commit -m "Add encrypted environment config"
git push

# Developer B: Pull and decrypt
git pull
dotenv-encrypt decrypt .env.enc --output .env.local
# Uses .env.local for development
```

## Package Overview

This project provides three complementary packages:

| Package | Use Case | Installation |
|---------|----------|--------------|
| **`DotEnv.SecretManager.MSBuild`** | **Automatic build integration** | `dotnet add package` |
| `DotEnvSecretManager` | Manual CLI encryption | `dotnet tool install -g` |
| `DotEnv.SecretManager.Core` | Programmatic library use | `dotnet add package` |

### Which Package Should I Use?

| Scenario | Recommended Package | Why |
|----------|-------------------|-----|
| **Team development** | `DotEnv.SecretManager.MSBuild` | Zero setup, automatic, consistent |
| **CI/CD pipelines** | `DotEnv.SecretManager.MSBuild` | One build command handles everything |
| **New projects** | `DotEnv.SecretManager.MSBuild` | Set-and-forget automation |
| Individual manual use | `DotEnvSecretManager` (CLI) | Full control, no project changes |
| Library integration | `DotEnv.SecretManager.Core` | Programmatic access |

## Security Features

- **AES-256-CBC encryption** with PBKDF2 key derivation
- **10,000 iterations** for key strengthening
- **Random salt and IV** for each encryption
- **Base64 encoding** for safe text storage
- **Password confirmation** for encryption operations
- **Secure password prompting** with masked input

## Git Workflow & Files

### Safe to Commit
- `.env.enc`, `.env.production.enc` (encrypted files)
- `README.md`, source code, tests

### Never Commit (add to .gitignore)
```gitignore
.env
.env.local
.env.development
.env.production
*.decrypted
decrypted.env
*.backup.*
```

## Security Best Practices

### Password Management
- Use **strong, unique passwords** for each project
- Store passwords in a **secure password manager**
- Consider using **key derivation** from master passwords
- **Rotate passwords** regularly in production

### File Management
- Always add `.env*` to `.gitignore` (except `.enc` files)
- **Delete decrypted files** after use in CI/CD
- Create **backups** of original files before encryption
- Use **environment-specific** encrypted files

### Team Collaboration
- Share **encrypted files** via git safely
- Share **passwords** via secure channels (not Slack/email)
- Use **different passwords** for different environments
- Document the **encryption/decryption process** for your team

### CI/CD Security
- Store passwords in **secure CI/CD secrets**
- Use **environment-specific** configurations
- **Clean up** decrypted files after deployment
- **Audit** who has access to encryption passwords

## Library Usage (Programmatic)

```csharp
using DotEnv.SecretManager.Core.Services;

// Setup services
var encryptionService = new AesEncryptionService();
var fileService = new EnvFileService();
var secretManager = new SecretManager(encryptionService, fileService);

// Encrypt a file
var result = await secretManager.EncryptFileAsync(".env", "password");
if (result.Success)
{
    Console.WriteLine($"Encrypted {result.ProcessedEntries} entries");
}

// Direct string encryption
string encrypted = await encryptionService.EncryptAsync("secret-value", "password");
string decrypted = await encryptionService.DecryptAsync(encrypted, "password");
```

## Development

### Running Tests
```bash
dotnet test
# All tests should pass
```

### Building from Source
```bash
dotnet build
dotnet pack
```

### File Structure
```
DotEnvSecretManager/
??? DotEnv.SecretManager.Core/         # Core library
??? DotEnv.SecretManager.CLI/          # CLI tool  
??? DotEnv.SecretManager.MSBuild/      # MSBuild integration
??? DotEnv.SecretManager.Tests/        # Unit tests
??? README.md
??? .gitignore
```

## Version History

- **v1.2.0** - Added MSBuild integration for automatic encryption
- **v1.1.0** - Enhanced CLI with info command, better UX, improved architecture
- **v1.0.0** - Initial release with core encryption/decryption functionality

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

MIT License - feel free to use this in your projects!

## Support

- **Issues**: Report bugs and feature requests on GitHub
- **Documentation**: Check the examples and help commands
- **Security**: Report security issues privately to maintainers