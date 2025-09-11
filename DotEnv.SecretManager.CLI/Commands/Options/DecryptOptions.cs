using DotEnv.SecretManager.CLI.Infrastructure;

namespace DotEnv.SecretManager.CLI.Commands.Options;

public class DecryptOptions
{
    public string InputFile { get; set; } = "";
    public string OutputFile { get; set; } = "";
    public string? Password { get; set; }
    public bool Force { get; set; }

    public static DecryptOptions? Parse(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleHelper.WriteError("Usage: dotenv-encrypt decrypt <file> [options]");
            ConsoleHelper.WriteSuggestion("Run 'dotenv-encrypt help' for more information.");
            return null;
        }

        var inputFile = args[1];
        var options = new DecryptOptions
        {
            InputFile = inputFile,
            OutputFile = inputFile.EndsWith(".enc", StringComparison.OrdinalIgnoreCase)
                ? inputFile.Substring(0, inputFile.Length - 4)
                : $"{inputFile}.decrypted"
        };

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--output" or "-o":
                    if (i + 1 < args.Length)
                    {
                        options.OutputFile = args[i + 1];
                        i++;
                    }
                    break;
                case "--password" or "-p":
                    if (i + 1 < args.Length)
                    {
                        options.Password = args[i + 1];
                        i++;
                    }
                    break;
                case "--force" or "-f":
                    options.Force = true;
                    break;
            }
        }

        return options;
    }
}