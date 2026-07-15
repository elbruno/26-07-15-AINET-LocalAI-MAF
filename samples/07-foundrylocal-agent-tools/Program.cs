using ElBruno.MAF.FoundryLocal;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

const string ModelEnvVar = "FOUNDRY_LOCAL_MODEL";
const string PromptEnvVar = "FOUNDRY_LOCAL_AGENT_PROMPT";
const string CleanupModelEnvVar = "FOUNDRY_LOCAL_CLEANUP_MODEL";
const string DefaultModelAlias = "phi-3.5-mini";
const string DefaultPrompt = "I'm in Pacific Standard Time. My bill is 42.50 and I want 18% tip. Use tools and return JSON with keys localTime, tipSummary, fact.";

var modelAlias = Environment.GetEnvironmentVariable(ModelEnvVar) ?? DefaultModelAlias;
var prompt = Environment.GetEnvironmentVariable(PromptEnvVar) ?? DefaultPrompt;
var cleanupOverride = ParseCleanupOverride(Environment.GetEnvironmentVariable(CleanupModelEnvVar));
var ct = CancellationToken.None;

Console.WriteLine("Foundry Local agent + tools sample");
Console.WriteLine("Step 1/6: Checking model cache status");
Console.WriteLine($"Model alias: {modelAlias}");
Console.WriteLine($"Prompt: {prompt}");

// Adapter options: local model alias and automatic download behavior.
var foundryOptions = Options.Create(new FoundryLocalOptions
{
    ModelAlias = modelAlias,
    DownloadIfMissing = true,
    UnloadOnExit = false
});

// Runtime defaults for the underlying local chat client.
var runtimeOptions = Options.Create(new ChatRuntimeOptions
{
    Temperature = 0.1,
    MaxOutputTokens = 256,
    Streaming = false
});

var lifecycle = new FoundryLocalModelLifecycleService(
    foundryOptions,
    runtimeOptions,
    NullLogger<FoundryLocalModelLifecycleService>.Instance);

using var adapterClient = new FoundryLocalChatClientAdapter(
    lifecycle,
    NullLogger<FoundryLocalChatClientAdapter>.Instance);

Console.WriteLine();
Console.WriteLine("Step 2/6: Preparing local model lifecycle");
await lifecycle.GetChatClientAsync(ct);
var diagnostics = lifecycle.GetDiagnosticsSnapshot();
Console.WriteLine(diagnostics.DownloadedThisSession
    ? "Model cache: not present. Downloaded during startup preparation."
    : "Model cache: already available locally.");

Console.WriteLine();
Console.WriteLine("Step 3/6: Creating agent-style chat client with tool invocation middleware");

// Wrap the adapter with MEAI function invocation middleware so the model
// can call .NET functions defined as AITools.
var agentClient = adapterClient
    .AsBuilder()
    .UseFunctionInvocation(NullLoggerFactory.Instance, cfg =>
    {
        cfg.MaximumIterationsPerRequest = 8;
        cfg.IncludeDetailedErrors = true;
    })
    .Build();

Console.WriteLine();
Console.WriteLine("Step 4/6: Registering tools");
var tools = AgentSampleTools.BuildTools();
Console.WriteLine($"Registered tools: {string.Join(", ", tools.Select(t => t.Name))}");

var options = new ChatOptions
{
    Instructions = "You are a concise local assistant. Use tools when useful, then return compact JSON only.",
    ToolMode = ChatToolMode.RequireAny,
    Temperature = 0.1f,
    Tools = tools
};

Console.WriteLine();
Console.WriteLine("Step 5/6: Running local agent turn");
var response = await agentClient.GetResponseAsync(prompt, options, ct);

Console.WriteLine();
Console.WriteLine("Agent response:");
Console.WriteLine(response.Text);

Console.WriteLine();
Console.WriteLine("Step 6/6: Cleanup");
var shouldCleanup = cleanupOverride ?? AskToDeleteDownloadedModel();
if (shouldCleanup)
{
    try
    {
        Console.WriteLine("Removing model from local cache...");
        var cleanupCatalog = await FoundryLocalManager.Instance.GetCatalogAsync();
        var cleanupModel = await cleanupCatalog.GetModelAsync(modelAlias);
        if (cleanupModel is null)
        {
            Console.WriteLine("Model alias was not found in local catalog for cleanup.");
        }
        else
        {
            await cleanupModel.RemoveFromCacheAsync();
            Console.WriteLine("Model cache removed.");
        }
    }
    finally
    {
        await lifecycle.DisposeAsync();
    }
}
else
{
    await lifecycle.DisposeAsync();
    Console.WriteLine("Done.");
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
    Console.Write("Delete downloaded model? [Y/n] ");
    var answer = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(answer))
    {
        return true;
    }

    return !answer.Equals("n", StringComparison.OrdinalIgnoreCase)
           && !answer.Equals("no", StringComparison.OrdinalIgnoreCase);
}
