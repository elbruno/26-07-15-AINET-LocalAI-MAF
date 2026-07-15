using ElBruno.MAF.FoundryLocal;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

const string ModelEnvVar = "FOUNDRY_LOCAL_MODEL";
const string PromptEnvVar = "FOUNDRY_LOCAL_AGENT_PROMPT";
const string DefaultModelAlias = "phi-3.5-mini";
const string DefaultPrompt = "I'm in Pacific Standard Time. My bill is 42.50 and I want 18% tip. Use tools and return JSON with keys localTime, tipSummary, fact.";

var modelAlias = Environment.GetEnvironmentVariable(ModelEnvVar) ?? DefaultModelAlias;
var prompt = Environment.GetEnvironmentVariable(PromptEnvVar) ?? DefaultPrompt;
var ct = CancellationToken.None;

Console.WriteLine("Foundry Local agent + tools sample");
Console.WriteLine("Step 1/5: Configuring local model lifecycle");
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
Console.WriteLine("Step 2/5: Creating agent-style chat client with tool invocation middleware");

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
Console.WriteLine("Step 3/5: Registering tools");
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
Console.WriteLine("Step 4/5: Running local agent turn");
var response = await agentClient.GetResponseAsync(prompt, options, ct);

Console.WriteLine();
Console.WriteLine("Agent response:");
Console.WriteLine(response.Text);

Console.WriteLine();
Console.WriteLine("Step 5/5: Cleanup");
await lifecycle.DisposeAsync();
Console.WriteLine("Done.");

return 0;
