# DotEnv Secret Manager

A secure .env file encryption tool for C#/.NET developers.

## What This Solves

- **Accidental commits**: Encrypt your .env files so secrets never leak into git
- **Team sharing**: Safely share encrypted configuration with your team
- **Environment security**: Keep secrets encrypted at rest

## Installation

### Option 1: Global .NET Tool (Recommended)
```bash
# Install from local build
dotnet pack
dotnet tool install -g --add-source ./bin/Release DotEnvSecretManager

# Use anywhere
dotenv-encrypt encrypt .env
```

### Option 2: Run from Source
```bash
# Clone and build
git clone https://github.com/iiSmitty/envcrypt
cd DotEnvSecretManager
dotnet build

# Run commands
dotnet run --project DotEnv.SecretManager.CLI encrypt .env
```

### Option 3: Use as Library
```bash
# Add to your project
dotnet add package DotEnv.SecretManager.Core

# Use in code
var secretManager = new SecretManager(encryptionService, fileService);
var result = await secretManager.EncryptFileAsync(".env", "password");
```

## Quick Start

### 1. Create a sample .env file
```bash
echo "API_KEY=sk_test_123456789" > .env
echo "DB_PASSWORD=super_secret_password" >> .env
echo "JWT_SECRET=my_jwt_secret_key" >> .env
```

### 2. Analyze your file
```bash
dotenv-encrypt info .env
```

### 3. Encrypt the file
```bash
dotenv-encrypt encrypt .env
# You'll be prompted for a password (with confirmation)
```

### 4. Decrypt when needed
```bash
dotenv-encrypt decrypt .env.enc
```

## Commands

### Core Commands

#### `encrypt` - Encrypt a .env file
```bash
# Basic encryption (prompts for password)
dotenv-encrypt encrypt .env

# Custom output file
dotenv-encrypt encrypt .env --output .env.production.enc

# Force overwrite existing files
dotenv-encrypt encrypt .env --force

# With password via command line (not recommended)
dotenv-encrypt encrypt .env --password mypassword
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

# Quick validation
dotenv-encrypt validate .env.enc --password mypassword
```

#### `info` - Analyze file contents
```bash
# Analyze any .env file
dotenv-encrypt info .env
dotenv-encrypt info .env.enc

# Shows: file size, entry counts, encryption status, preview
```

### Utility Commands

#### `help` - Show usage information
```bash
dotenv-encrypt help           # General help
dotenv-encrypt examples       # Usage examples  
```

#### `version` - Show version and info
```bash
dotenv-encrypt version
```

## Command Options

### Global Options
- `--help, -h` - Show help
- `--version, -v` - Show version

### Encrypt/Decrypt Options
- `--output, -o <file>` - Output file path
- `--password, -p <pwd>` - Password (prompts if not provided)
- `--force, -f` - Overwrite existing files without confirmation

## Security Features

- **AES-256-CBC encryption** with PBKDF2 key derivation
- **10,000 iterations** for key strengthening
- **Random salt and IV** for each encryption
- **Base64 encoding** for safe text storage
- **Password confirmation** for encryption operations
- **Secure password prompting** with masked input

## Example Output

### File Analysis
```
PS> dotenv-encrypt info .env

Analyzing: .env

File Information:
  Size: 2.1 KB
  Modified: 2025-09-11 18:38:05
  Lines: 72

Content Analysis:
  Total entries: 58
  Key-value pairs: 41
  Comments: 17
  Encrypted values: 0
  Plain text values: 41

Environment Variables:
  [PLAIN] DB_HOST = localhost
  [PLAIN] API_KEY = sk_test_1234567890abcdef
  ...
```

### Encryption Process
```
PS> dotenv-encrypt encrypt .env

Enter encryption password: ********
Confirm password: ********
Encrypting: .env
Output to: .env.enc

Progress: Processing file...
Encryption completed successfully!

Input file:        .env
Output file:       .env.enc
Entries encrypted: 39
Duration:          535ms
File size:         4.6 KB

Security Reminders:
  * Store your password safely - it cannot be recovered
  * Encrypted .enc files are safe to commit to git
  * Consider backing up the original file
```

## Typical Workflow

### Development Team Setup
```bash
# 1. Developer encrypts local .env
dotenv-encrypt encrypt .env
# Creates .env.enc (safe to commit)

# 2. Commit encrypted file
git add .env.enc
git commit -m "Add encrypted environment config"
git push

# 3. Team member pulls and decrypts
git pull
dotenv-encrypt decrypt .env.enc --output .env.local
# Uses .env.local for development
```

### CI/CD Pipeline
```bash
# In your deployment script
dotenv-encrypt decrypt .env.production.enc --output .env --password $DECRYPT_PASSWORD
# Deploy with decrypted environment
```

## File Structure

```
DotEnvSecretManager/
??? DotEnv.SecretManager.Core/         # Core library (NuGet package)
??? DotEnv.SecretManager.CLI/          # CLI tool (Global .NET tool)  
??? DotEnv.SecretManager.Tests/        # Unit tests
??? README.md
??? .gitignore
```

## Git Workflow & Files

### Safe to Commit
- `.env.enc`, `.env.production.enc` (encrypted files)
- `README.md`, source code, tests

### Never Commit (add to .gitignore)
```gitignore
.env
.env.local
.env.development
*.decrypted
decrypted.env
*.backup.*
```

## Library Usage (for Developers)

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

## Security Best Practices

### Password Management
- Use **strong, unique passwords** for each project
- Store passwords in a **secure password manager**
- Consider using **key derivation** from master passwords
- **Rotate passwords** regularly in production

### File Management
- Always add `.env*` to `.gitignore` (except `.enc` files)
- **Delete decrypted files** after use
- Create **backups** of original files before encryption
- Use **environment-specific** encrypted files (`.env.production.enc`)

### Team Collaboration
- Share **encrypted files** via git safely
- Share **passwords** via secure channels (not Slack/email)
- Use **different passwords** for different environments
- Document the **encryption/decryption process** for your team

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

### Contributing
1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## Version History

- **v1.1.0** - Enhanced CLI with info command, better UX, improved architecture
- **v1.0.0** - Initial release with core encryption/decryption functionality

## License

MIT License - feel free to use this in your projects!

## Support

- **Issues**: Report bugs and feature requests on GitHub
- **Documentation**: Check the examples and help commands
- **Security**: Report security issues privately to maintainers