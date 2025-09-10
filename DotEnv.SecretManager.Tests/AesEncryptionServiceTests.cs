using DotEnv.SecretManager.Core.Services;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;
using DotEnv.SecretManager.Core.Models;
using System.Collections.Generic;
using System.IO;

namespace DotEnv.SecretManager.Tests;

public class AesEncryptionServiceTests
{
    private readonly AesEncryptionService _encryptionService;

    public AesEncryptionServiceTests()
    {
        _encryptionService = new AesEncryptionService();
    }

    [Fact]
    public async Task EncryptAsync_ValidInput_ShouldReturnEncryptedString()
    {
        // Arrange
        var plaintext = "Hello, World!";
        var password = "test_password_123";

        // Act
        var encrypted = await _encryptionService.EncryptAsync(plaintext, password);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(plaintext);
        encrypted.Length.Should().BeGreaterThan(plaintext.Length);
    }

    [Fact]
    public async Task DecryptAsync_ValidEncryptedString_ShouldReturnOriginalPlaintext()
    {
        // Arrange
        var plaintext = "Secret API Key: sk_test_123456789";
        var password = "strong_password_456";

        // Act
        var encrypted = await _encryptionService.EncryptAsync(plaintext, password);
        var decrypted = await _encryptionService.DecryptAsync(encrypted, password);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public async Task DecryptAsync_WrongPassword_ShouldThrowException()
    {
        // Arrange
        var plaintext = "Secret data";
        var correctPassword = "correct_password";
        var wrongPassword = "wrong_password";

        var encrypted = await _encryptionService.EncryptAsync(plaintext, correctPassword);

        // Act & Assert
        await FluentActions
            .Invoking(() => _encryptionService.DecryptAsync(encrypted, wrongPassword))
            .Should()
            .ThrowAsync<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public async Task ValidateAsync_CorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var plaintext = "Test value";
        var password = "validation_password";
        var encrypted = await _encryptionService.EncryptAsync(plaintext, password);

        // Act
        var isValid = await _encryptionService.ValidateAsync(encrypted, password);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_IncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var plaintext = "Test value";
        var correctPassword = "correct_password";
        var incorrectPassword = "incorrect_password";
        var encrypted = await _encryptionService.EncryptAsync(plaintext, correctPassword);

        // Act
        var isValid = await _encryptionService.ValidateAsync(encrypted, incorrectPassword);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task EncryptAsync_InvalidPlaintext_ShouldThrowArgumentException(string? plaintext)
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _encryptionService.EncryptAsync(plaintext!, "password"))
            .Should()
            .ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task EncryptAsync_InvalidPassword_ShouldThrowArgumentException(string? password)
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _encryptionService.EncryptAsync("plaintext", password!))
            .Should()
            .ThrowAsync<ArgumentException>();
    }
}

public class EnvFileServiceTests : IDisposable
{
    private readonly EnvFileService _envFileService;
    private readonly string _testDirectory;

    public EnvFileServiceTests()
    {
        _envFileService = new EnvFileService();
        _testDirectory = Path.Combine(Path.GetTempPath(), "dotenv-test", Path.GetRandomFileName());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task ParseFileAsync_ValidEnvFile_ShouldParseCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, ".env");
        var content = @"# Configuration file
API_KEY=secret123
DB_HOST=localhost
DB_PORT=5432
QUOTED_VALUE=""value with spaces""
EMPTY_VALUE=
# Another comment
DEBUG=true";

        await File.WriteAllTextAsync(filePath, content);

        // Act
        var entries = await _envFileService.ParseFileAsync(filePath);

        // Assert
        entries.Should().HaveCount(8); // Including comments

        var apiKeyEntry = entries.Find(e => e.Key == "API_KEY");
        apiKeyEntry.Should().NotBeNull();
        apiKeyEntry!.Value.Should().Be("secret123");

        var quotedEntry = entries.Find(e => e.Key == "QUOTED_VALUE");
        quotedEntry!.Value.Should().Be("value with spaces");
    }

    [Fact]
    public async Task WriteFileAsync_ValidEntries_ShouldWriteCorrectFormat()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "output.env");
        var entries = new List<EnvEntry>
        {
            new("", "") { Comment = "Test configuration" },
            new("API_KEY", "secret123"),
            new("QUOTED_VALUE", "value with spaces"),
            new("DEBUG", "true") { Comment = "Enable debug mode" }
        };

        // Act
        await _envFileService.WriteFileAsync(filePath, entries);

        // Assert
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain("# Test configuration");
        content.Should().Contain("API_KEY=secret123");
        content.Should().Contain("QUOTED_VALUE=\"value with spaces\"");
        content.Should().Contain("DEBUG=true # Enable debug mode");
    }

    [Fact]
    public async Task FileExistsAsync_ExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "existing.env");
        await File.WriteAllTextAsync(filePath, "TEST=value");

        // Act
        var exists = await _envFileService.FileExistsAsync(filePath);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task FileExistsAsync_NonExistingFile_ShouldReturnFalse()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "non-existing.env");

        // Act
        var exists = await _envFileService.FileExistsAsync(filePath);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CreateBackupAsync_ExistingFile_ShouldCreateBackup()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "backup-test.env");
        var originalContent = "ORIGINAL=value";
        await File.WriteAllTextAsync(filePath, originalContent);

        // Act
        var backupPath = await _envFileService.CreateBackupAsync(filePath);

        // Assert
        File.Exists(backupPath).Should().BeTrue();
        var backupContent = await File.ReadAllTextAsync(backupPath);
        backupContent.Should().Be(originalContent);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}