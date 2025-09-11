using DotEnv.SecretManager.CLI.Infrastructure;

namespace DotEnv.SecretManager.CLI.Commands;

public class CommandRunner
{
    private readonly AppServices _services;
    private readonly string _version;

    public CommandRunner(AppServices services, string version)
    {
        _services = services;
        _version = version;
    }

    public async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            HelpCommand.ShowHelp(_version);
            return 1;
        }

        string command = args[0].ToLower();

        return command switch
        {
            "encrypt" => await new EncryptCommand(_services).ExecuteAsync(args),
            "decrypt" => await new DecryptCommand(_services).ExecuteAsync(args),
            "validate" => await new ValidateCommand(_services).ExecuteAsync(args),
            "info" => await new InfoCommand(_services).ExecuteAsync(args),
            "version" or "--version" or "-v" => VersionCommand.ShowVersion(_version),
            "help" or "--help" or "-h" => HelpCommand.ShowHelp(_version),
            "examples" => HelpCommand.ShowExamples(),
            _ => HandleUnknownCommand(command)
        };
    }

    private static int HandleUnknownCommand(string command)
    {
        ConsoleHelper.WriteError($"Unknown command: {command}");
        Console.WriteLine("Run 'dotenv-encrypt help' for usage information.");
        return 1;
    }
}