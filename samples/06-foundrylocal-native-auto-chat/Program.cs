using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;

const string ModelEnvVar = "FOUNDRY_LOCAL_MODEL";
const string PromptEnvVar = "FOUNDRY_LOCAL_PROMPT";
const string DefaultModelAlias = "qwen2.5-0.5b";
const string DefaultPrompt = "Why is the sky blue?";

var modelAlias = Environment.GetEnvironmentVariable(ModelEnvVar) ?? DefaultModelAlias;
var prompt = Environment.GetEnvironmentVariable(PromptEnvVar) ?? DefaultPrompt;
var ct = CancellationToken.None;

var config = new Configuration
{
    AppName = "dotnet-local-ai-native-auto-chat",
    LogLevel = LogLevel.Information
};

Console.WriteLine("Foundry Local native auto chat sample");
Console.WriteLine("Initializing SDK and local runtime...");
await FoundryLocalManager.CreateAsync(config, NullLogger.Instance);
var manager = FoundryLocalManager.Instance;

var eps = manager.DiscoverEps();
const int maxNameLength = 30;
Console.WriteLine();
Console.WriteLine("Available execution providers:");
Console.WriteLine($"  {"Name".PadRight(maxNameLength)}  Registered");
Console.WriteLine($"  {new string('─', maxNameLength)}  ----------");
foreach (var ep in eps)
{
    Console.WriteLine($"  {ep.Name.PadRight(maxNameLength)}  {ep.IsRegistered}");
}

Console.WriteLine();
Console.WriteLine("Downloading/registering execution providers:");
if (eps.Length > 0)
{
    var currentEp = string.Empty;
    await manager.DownloadAndRegisterEpsAsync((epName, percent) =>
    {
        if (!string.Equals(epName, currentEp, StringComparison.Ordinal))
        {
            if (!string.IsNullOrEmpty(currentEp))
            {
                Console.WriteLine();
            }

            currentEp = epName;
        }

        Console.Write($"\r  {epName.PadRight(maxNameLength)}  {percent,6:F1}%");
    });
    Console.WriteLine();
}
else
{
    Console.WriteLine("  No execution providers to download.");
}

var catalog = await manager.GetCatalogAsync();
var model = await catalog.GetModelAsync(modelAlias);
if (model is null)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine($"Model '{modelAlias}' was not found in the local catalog.");
    Console.Error.WriteLine("Set FOUNDRY_LOCAL_MODEL to an installed alias and try again.");
    manager.Dispose();
    return 1;
}

Console.WriteLine();
Console.WriteLine($"Resolved model: {model.Alias} ({model.Id})");

await model.DownloadAsync(progress =>
{
    Console.Write($"\rDownloading model: {progress,6:F2}%");
    if (progress >= 100f)
    {
        Console.WriteLine();
    }
});

Console.Write($"Loading model {model.Id}...");
await model.LoadAsync();
Console.WriteLine("done.");

try
{
    var chatClient = await model.GetChatClientAsync();
    var messages = new List<ChatMessage>
    {
        new() { Role = "user", Content = prompt }
    };

    Console.WriteLine();
    Console.WriteLine("Chat completion response:");
    await foreach (var chunk in chatClient.CompleteChatStreamingAsync(messages, ct))
    {
        if (chunk.Choices is null || chunk.Choices.Count == 0)
        {
            continue;
        }

        var content = chunk.Choices[0].Message?.Content;
        if (string.IsNullOrEmpty(content))
        {
            continue;
        }

        Console.Write(content);
        Console.Out.Flush();
    }

    Console.WriteLine();
}
finally
{
    Console.WriteLine();
    Console.WriteLine("Unloading model...");
    await model.UnloadAsync();
    Console.WriteLine("Model unloaded.");
    manager.Dispose();
}

return 0;
