using DotEnv.SecretManager.CLI.Infrastructure;

namespace DotEnv.SecretManager.CLI.Commands.Options;

public class ValidateOptions
{
    public string InputFile { get; set; } = "";
    public string? Password { get; set; }

    public static ValidateOptions? Parse(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleHelper.WriteError("Usage: dotenv-encrypt validate <file> [options]");
            return null;
        }

        var options = new ValidateOptions
        {
            InputFile = args[1]
        };

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--password" or "-p":
                    if (i + 1 < args.Length)
                    {
                        options.Password = args[i + 1];
                        i++;
                    }
                    break;
            }
        }

        return options;
    }
}