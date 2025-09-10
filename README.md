# DotEnv Secret Manager

A secure .env file encryption tool for C#/.NET developers.

## What This Solves

- **Accidental commits**: Encrypt your .env files so secrets never leak into git
- **Team sharing**: Safely share encrypted configuration with your team
- **Environment security**: Keep secrets encrypted at rest

## Quick Start

### 1. Build the project

```bash
dotnet build
```

### 2. Create a sample .env file

```bash
# Create a test .env file
echo "API_KEY=sk_test_123456789" > .env
echo "DB_PASSWORD=super_secret_password" >> .env
echo "JWT_SECRET=my_jwt_secret_key" >> .env
```

### 3. Encrypt the file

```bash
dotnet run --project DotEnv.SecretManager.CLI encrypt .env
# You'll be prompted for a password
```

### 4. Decrypt when needed

```bash
dotnet run --project DotEnv.SecretManager.CLI decrypt .env.enc
```

## Commands

### Encrypt
```bash
# Encrypt with password prompt
dotenv-encrypt encrypt .env

# Encrypt with custom output
dotenv-encrypt encrypt .env --output .env.production.enc

# Encrypt with password via command line (not recommended for production)
dotenv-encrypt encrypt .env --password mypassword
```

### Decrypt
```bash
# Decrypt with password prompt
dotenv-encrypt decrypt .env.enc

# Decrypt with custom output
dotenv-encrypt decrypt .env.enc --output .env.local
```

### Validate
```bash
# Check if encrypted file is valid and can be decrypted
dotenv-encrypt validate .env.enc
```

## Security Features

- **AES-256-CBC encryption** with PBKDF2 key derivation
- **10,000 iterations** for key strengthening
- **Random salt and IV** for each encryption
- **Base64 encoding** for safe text storage

## File Structure

```
DotEnvSecretManager/
??? DotEnv.SecretManager.Core/     # Core encryption logic
??? DotEnv.SecretManager.CLI/      # Command-line interface
??? DotEnv.SecretManager.Tests/    # Unit tests
```

## Running Tests

```bash
dotnet test
```

## Example Integration (Future Phase)

```csharp
// Future: Use as a library in your .NET apps
var secretManager = new SecretManager(encryptionService, fileService);
var result = await secretManager.DecryptFileAsync(".env.enc", password);
```

## Important Security Notes

1. **Store passwords safely** - Use a password manager or secure vault
2. **Never commit passwords** - Always prompt or use environment variables
3. **Backup original files** - Keep secure backups of your .env files
4. **Add to .gitignore**: 
   ```
   .env
   .env.local
   *.backup.*
   ```
5. **Include encrypted files**: 
   ```
   # These are safe to commit
   .env.enc
   .env.production.enc
   ```

## Roadmap

- [x] **Phase 1**: Basic AES encryption/decryption
- [ ] **Phase 2**: RSA public/private key support
- [ ] **Phase 3**: NuGet package and MSBuild integration
- [ ] **Phase 4**: Visual Studio extension
- [ ] **Phase 5**: Azure Key Vault integration

## Contributing

This project is in active development. Feel free to open issues and suggest features!

## License

MIT License - feel free to use this in your projects!