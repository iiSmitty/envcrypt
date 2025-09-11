using DotEnv.SecretManager.CLI.Infrastructure;

namespace DotEnv.SecretManager.CLI.Commands;

public class InfoCommand : BaseCommand
{
    public InfoCommand(AppServices services) : base(services) { }

    public override async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleHelper.WriteError("Usage: dotenv-encrypt info <file>");
            return 1;
        }

        var filePath = args[1];

        try
        {
            if (!CheckFileExists(filePath)) return 1;

            ConsoleHelper.WriteInfo($"Analyzing: {filePath}");
            Console.WriteLine();

            var entries = await Services.EnvFileService.ParseFileAsync(filePath);
            var fileInfo = new FileInfo(filePath);

            // Basic file info
            ConsoleHelper.WriteInfo("File Information:");
            ConsoleHelper.WriteInfo($"  Size: {ConsoleHelper.GetFileSize(filePath)}");
            ConsoleHelper.WriteInfo($"  Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            ConsoleHelper.WriteInfo($"  Lines: {File.ReadAllLines(filePath).Length}");
            Console.WriteLine();

            // Entry analysis
            var keyValueEntries = entries.Where(e => !string.IsNullOrEmpty(e.Key)).ToList();
            var commentEntries = entries.Where(e => string.IsNullOrEmpty(e.Key) && !string.IsNullOrEmpty(e.Comment)).ToList();
            var encryptedEntries = keyValueEntries.Where(e => e.IsEncrypted).ToList();

            ConsoleHelper.WriteInfo("Content Analysis:");
            ConsoleHelper.WriteInfo($"  Total entries: {entries.Count}");
            ConsoleHelper.WriteInfo($"  Key-value pairs: {keyValueEntries.Count}");
            ConsoleHelper.WriteInfo($"  Comments: {commentEntries.Count}");
            ConsoleHelper.WriteInfo($"  Encrypted values: {encryptedEntries.Count}");
            ConsoleHelper.WriteInfo($"  Plain text values: {keyValueEntries.Count - encryptedEntries.Count}");
            Console.WriteLine();

            // Show entries
            if (keyValueEntries.Any())
            {
                ConsoleHelper.WriteInfo("Environment Variables:");
                foreach (var entry in keyValueEntries.Take(10))
                {
                    var status = entry.IsEncrypted ? "[ENCRYPTED]" : "[PLAIN]";
                    var valuePreview = entry.IsEncrypted ? "[ENCRYPTED DATA]" :
                                     entry.Value.Length > 20 ? $"{entry.Value.Substring(0, 20)}..." : entry.Value;
                    ConsoleHelper.WriteInfo($"  {status} {entry.Key} = {valuePreview}");
                }

                if (keyValueEntries.Count > 10)
                {
                    ConsoleHelper.WriteDim($"  ... and {keyValueEntries.Count - 10} more entries");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Analysis error: {ex.Message}");
            return 1;
        }
    }
}