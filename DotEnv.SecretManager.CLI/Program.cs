using DotEnv.SecretManager.Core.Services;

namespace DotEnv.SecretManager.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Setup services
            var encryptionService = new AesEncryptionService();
            var envFileService = new EnvFileService();
            var secretManager = new Core.Services.SecretManager(encryptionService, envFileService);

            // Parse command line arguments
            if (args.Length == 0)
            {
                ShowHelp();
                return 1;
            }

            string command = args[0].ToLower();

            switch (command)
            {
                case "encrypt":
                    return await HandleEncrypt(secretManager, args);
                case "decrypt":
                    return await HandleDecrypt(secretManager, args);
                case "validate":
                    return await HandleValidate(secretManager, args);
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    return 0;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowHelp();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> HandleEncrypt(Core.Services.SecretManager secretManager, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotenv-encrypt encrypt <file> [--output <output_file>] [--password <password>]");
            return 1;
        }

        string inputFile = args[1];
        string? outputFile = null;
        string? password = null;

        // Parse additional arguments
        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--output":
                case "-o":
                    if (i + 1 < args.Length)
                    {
                        outputFile = args[i + 1];
                        i++;
                    }
                    break;
                case "--password":
                case "-p":
                    if (i + 1 < args.Length)
                    {
                        password = args[i + 1];
                        i++;
                    }
                    break;
            }
        }

        // Validate input file exists
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: File not found: {inputFile}");
            return 1;
        }

        // Get password if not provided
        if (string.IsNullOrEmpty(password))
        {
            password = PromptForPassword("Enter encryption password: ");
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Error: Password cannot be empty");
                return 1;
            }
        }

        Console.WriteLine($"Encrypting {inputFile}...");

        var result = await secretManager.EncryptFileAsync(inputFile, password, outputFile);

        if (result.Success)
        {
            Console.WriteLine($"Success: Encrypted {result.ProcessedEntries} entries");
            Console.WriteLine($"Output: {result.OutputFilePath}");
            Console.WriteLine($"Duration: {result.Duration.TotalMilliseconds:F0}ms");
            Console.WriteLine();
            Console.WriteLine("Important:");
            Console.WriteLine("  * Store your password safely - it cannot be recovered");
            Console.WriteLine("  * Encrypted .enc files are safe to commit to git");
            Console.WriteLine("  * Consider backing up the original file");
            return 0;
        }
        else
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
            return 1;
        }
    }

    private static async Task<int> HandleDecrypt(Core.Services.SecretManager secretManager, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotenv-encrypt decrypt <file> [--output <output_file>] [--password <password>]");
            return 1;
        }

        string inputFile = args[1];
        string? outputFile = null;
        string? password = null;

        // Parse additional arguments
        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--output":
                case "-o":
                    if (i + 1 < args.Length)
                    {
                        outputFile = args[i + 1];
                        i++;
                    }
                    break;
                case "--password":
                case "-p":
                    if (i + 1 < args.Length)
                    {
                        password = args[i + 1];
                        i++;
                    }
                    break;
            }
        }

        // Validate input file exists
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: File not found: {inputFile}");
            return 1;
        }

        // Get password if not provided
        if (string.IsNullOrEmpty(password))
        {
            password = PromptForPassword("Enter decryption password: ");
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Error: Password cannot be empty");
                return 1;
            }
        }

        Console.WriteLine($"Decrypting {inputFile}...");

        var result = await secretManager.DecryptFileAsync(inputFile, password, outputFile);

        if (result.Success)
        {
            Console.WriteLine($"Success: Decrypted {result.ProcessedEntries} entries");
            Console.WriteLine($"Output: {result.OutputFilePath}");
            Console.WriteLine($"Duration: {result.Duration.TotalMilliseconds:F0}ms");
            Console.WriteLine();
            Console.WriteLine("Security reminder:");
            Console.WriteLine("  * The decrypted file contains plaintext secrets");
            Console.WriteLine("  * Ensure it's in your .gitignore");
            Console.WriteLine("  * Delete it when no longer needed");
            return 0;
        }
        else
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
            return 1;
        }
    }

    private static async Task<int> HandleValidate(Core.Services.SecretManager secretManager, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotenv-encrypt validate <file> [--password <password>]");
            return 1;
        }

        string inputFile = args[1];
        string? password = null;

        // Parse additional arguments
        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--password":
                case "-p":
                    if (i + 1 < args.Length)
                    {
                        password = args[i + 1];
                        i++;
                    }
                    break;
            }
        }

        // Validate input file exists
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: File not found: {inputFile}");
            return 1;
        }

        // Get password if not provided
        if (string.IsNullOrEmpty(password))
        {
            password = PromptForPassword("Enter password to validate: ");
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Error: Password cannot be empty");
                return 1;
            }
        }

        Console.WriteLine($"Validating {inputFile}...");

        bool isValid = await secretManager.ValidateFileAsync(inputFile, password);

        if (isValid)
        {
            Console.WriteLine("Success: File is valid and can be decrypted with the provided password");
            return 0;
        }
        else
        {
            Console.WriteLine("Error: File cannot be decrypted with the provided password");
            return 1;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("DotEnv Secret Manager - Encrypt and decrypt .env files safely");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotenv-encrypt encrypt <file> [--output <output_file>] [--password <password>]");
        Console.WriteLine("  dotenv-encrypt decrypt <file> [--output <output_file>] [--password <password>]");
        Console.WriteLine("  dotenv-encrypt validate <file> [--password <password>]");
        Console.WriteLine("  dotenv-encrypt help");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  encrypt    Encrypt a .env file");
        Console.WriteLine("  decrypt    Decrypt an encrypted .env file");
        Console.WriteLine("  validate   Validate that an encrypted file can be decrypted");
        Console.WriteLine("  help       Show this help message");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --output, -o    Output file path");
        Console.WriteLine("  --password, -p  Password (will prompt if not provided)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotenv-encrypt encrypt .env");
        Console.WriteLine("  dotenv-encrypt encrypt .env --output .env.production.enc");
        Console.WriteLine("  dotenv-encrypt decrypt .env.enc");
        Console.WriteLine("  dotenv-encrypt validate .env.enc");
    }

    private static string PromptForPassword(string prompt)
    {
        Console.Write(prompt);
        var password = "";
        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b");
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                password += keyInfo.KeyChar;
                Console.Write("*");
            }
        } while (keyInfo.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }
}