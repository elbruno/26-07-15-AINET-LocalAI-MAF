using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.RegularExpressions;

const string ModelEnvVar = "FOUNDRY_LOCAL_MODEL";
const string PromptEnvVar = "FOUNDRY_LOCAL_PROMPT";
const string CleanupModelEnvVar = "FOUNDRY_LOCAL_CLEANUP_MODEL";
const string DefaultModelAlias = "phi-3.5-mini";
const string DefaultPrompt = "Why is the sky blue?";

var modelAlias = Environment.GetEnvironmentVariable(ModelEnvVar) ?? DefaultModelAlias;
var prompt = Environment.GetEnvironmentVariable(PromptEnvVar) ?? DefaultPrompt;
var cleanupOverride = ParseCleanupOverride(Environment.GetEnvironmentVariable(CleanupModelEnvVar));
var ct = CancellationToken.None;

var config = new Configuration
{
    AppName = "dotnet-local-ai-native-auto-chat",
    LogLevel = LogLevel.Information
};

Console.WriteLine("Foundry Local native auto chat sample");
Console.WriteLine("Step 1/6: Initializing SDK and local runtime...");
Console.WriteLine($"Question: {prompt}");
await FoundryLocalManager.CreateAsync(config, NullLogger.Instance);
var manager = FoundryLocalManager.Instance;
IModel? loadedModel = null;

try
{
    var eps = manager.DiscoverEps();
    const int maxNameLength = 30;
    Console.WriteLine();
    Console.WriteLine("Step 2/6: Discovering execution providers");
    Console.WriteLine($"  {"Name".PadRight(maxNameLength)}  Registered");
    Console.WriteLine($"  {new string('─', maxNameLength)}  ----------");
    foreach (var ep in eps)
    {
        Console.WriteLine($"  {ep.Name.PadRight(maxNameLength)}  {ep.IsRegistered}");
    }

    Console.WriteLine();
    Console.WriteLine("Step 3/6: Downloading/registering execution providers");
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

    Console.WriteLine();
    Console.WriteLine("Step 4/6: Resolving model + best variant for this machine");
    var catalog = await manager.GetCatalogAsync();
    var model = await catalog.GetModelAsync(modelAlias);
    if (model is null)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine($"Model '{modelAlias}' was not found in the local catalog.");
        Console.Error.WriteLine("Set FOUNDRY_LOCAL_MODEL to an installed alias and try again.");
        return 1;
    }

    Console.WriteLine($"Resolved model alias: {model.Alias}");
    var refreshedEps = manager.DiscoverEps();
    var selectedVariant = SelectBestVariantForMachine(model, refreshedEps);
    if (selectedVariant is not null)
    {
        model.SelectVariant(selectedVariant);
        Console.WriteLine($"Selected variant: {selectedVariant.Id} ({selectedVariant.Info.Runtime?.DeviceType})");
    }
    else
    {
        Console.WriteLine("Selected variant: SDK default");
    }

    Console.WriteLine();
    Console.WriteLine("Step 5/6: Downloading/loading model");
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
    loadedModel = model;
    Console.WriteLine("done.");

    var chatClient = await model.GetChatClientAsync();
    chatClient.Settings.Temperature = 0.0f;
    chatClient.Settings.TopP = 0.1f;
    chatClient.Settings.MaxTokens = 96;
    chatClient.Settings.RandomSeed = 42;

    Console.WriteLine();
    Console.WriteLine("Step 6/6: Generating answer");
    var answer = await AskQuestionWithQualityGuardAsync(chatClient, prompt, ct);
    Console.WriteLine();
    Console.WriteLine("Answer:");
    Console.WriteLine(answer);

    var shouldCleanup = cleanupOverride ?? AskToDeleteDownloadedModel();
    if (shouldCleanup && loadedModel is not null)
    {
        Console.WriteLine();
        Console.WriteLine("Removing model from local cache...");
        await loadedModel.RemoveFromCacheAsync();
        Console.WriteLine("Model cache removed.");
    }
}
finally
{
    if (loadedModel is not null)
    {
        Console.WriteLine();
        Console.WriteLine("Unloading model...");
        await loadedModel.UnloadAsync();
        Console.WriteLine("Model unloaded.");
    }

    manager.Dispose();
}

return 0;

static bool? ParseCleanupOverride(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    return null;
}

static bool AskToDeleteDownloadedModel()
{
    Console.WriteLine();
    Console.Write("Delete downloaded model? [Y/n] ");
    var answer = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(answer))
    {
        return true;
    }

    return !answer.Equals("n", StringComparison.OrdinalIgnoreCase)
           && !answer.Equals("no", StringComparison.OrdinalIgnoreCase);
}

static async Task<string> AskQuestionWithQualityGuardAsync(
    OpenAIChatClient chatClient,
    string prompt,
    CancellationToken ct)
{
    var primaryMessages = new List<ChatMessage>
    {
        new()
        {
            Role = "system",
            Content = "Answer only the user's question. Reply in plain English using ASCII characters and common punctuation. Use exactly one short sentence."
        },
        new() { Role = "user", Content = prompt }
    };

    var first = await chatClient.CompleteChatAsync(primaryMessages, ct);
    var firstText = ExtractFirstChoice(first);
    firstText = NormalizeAnswer(firstText);

    var promptKeywords = ExtractKeywords(prompt);
    if (LooksGoodAnswer(firstText, promptKeywords))
    {
        return firstText!;
    }

    Console.WriteLine("Primary response looked malformed or off-topic. Retrying once...");

    var retryMessages = new List<ChatMessage>
    {
        new()
        {
            Role = "system",
            Content = "Reply in plain English with ASCII characters. Answer only the user's question in one short factual sentence."
        },
        new() { Role = "user", Content = prompt }
    };
    var retry = await chatClient.CompleteChatAsync(retryMessages, ct);
    var retryText = NormalizeAnswer(ExtractFirstChoice(retry));
    if (!LooksGoodAnswer(retryText, promptKeywords))
    {
        throw new InvalidOperationException(
            "Model response was still malformed/off-topic. Try a different model alias via FOUNDRY_LOCAL_MODEL.");
    }

    return retryText!;
}

static string? ExtractFirstChoice(Betalgo.Ranul.OpenAI.ObjectModels.ResponseModels.ChatCompletionCreateResponse response)
{
    return response.Choices is { Count: > 0 }
        ? response.Choices[0].Message?.Content
        : null;
}

static string? NormalizeAnswer(string? text)
{
    if (string.IsNullOrWhiteSpace(text))
    {
        return null;
    }

    var normalized = text.Trim();
    normalized = Regex.Replace(normalized, @"^[^\p{L}\p{N}]+", string.Empty);
    normalized = Regex.Replace(normalized, @"\s+", " ");
    return normalized.Trim();
}

static bool LooksGoodAnswer(string? answer, IReadOnlyList<string> promptKeywords)
{
    if (string.IsNullOrWhiteSpace(answer))
    {
        return false;
    }

    var trimmed = answer.Trim();
    if (trimmed.Length < 12 || trimmed.Length > 220)
    {
        return false;
    }

    var asciiLetters = trimmed.Count(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
    var digits = trimmed.Count(char.IsDigit);
    var asciiRatio = asciiLetters / (double)trimmed.Length;
    var nonAsciiRatio = trimmed.Count(c => c > 127) / (double)trimmed.Length;

    if (asciiRatio < 0.40 || nonAsciiRatio > 0.10 || digits > asciiLetters)
    {
        return false;
    }

    if (promptKeywords.Count == 0)
    {
        return true;
    }

    return promptKeywords.Any(keyword => trimmed.Contains(keyword, StringComparison.OrdinalIgnoreCase));
}

static List<string> ExtractKeywords(string prompt)
{
    var stopwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "what", "when", "where", "which", "this", "that", "with", "from", "into", "about", "your", "their",
        "there", "have", "does", "doing", "why", "how", "is", "are", "the", "and", "for", "you"
    };

    var words = Regex.Matches(prompt, @"[A-Za-z]{3,}")
        .Select(m => m.Value.ToLowerInvariant())
        .Where(w => !stopwords.Contains(w))
        .Distinct()
        .Take(6)
        .ToList();

    return words;
}

static IModel? SelectBestVariantForMachine(IModel model, IReadOnlyList<EpInfo> eps)
{
    var variants = model.Variants?.ToList();
    if (variants is null || variants.Count == 0)
    {
        return null;
    }

    var hasRegisteredGpu = eps.Any(ep =>
        ep.IsRegistered && ep.Name.Contains("gpu", StringComparison.OrdinalIgnoreCase));

    static int RankVariant(IModel variant, bool preferGpu)
    {
        var device = variant.Info.Runtime?.DeviceType;
        return (preferGpu, device) switch
        {
            (true, DeviceType.GPU) => 0,
            (true, DeviceType.CPU) => 1,
            (false, DeviceType.CPU) => 0,
            (false, DeviceType.GPU) => 1,
            _ => 2
        };
    }

    return variants
        .OrderBy(v => RankVariant(v, hasRegisteredGpu))
        .ThenBy(v => v.Id, StringComparer.OrdinalIgnoreCase)
        .FirstOrDefault();
}
