using System.ClientModel;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;

const string DefaultBaseUrl = "http://127.0.0.1:5273/v1";
const string DefaultModel = "qwen2.5-0.5b";
const string DefaultApiKey = "local-dev-key";
const string DefaultPromptVariant = "eli5";

var configuredBaseUrl = Environment.GetEnvironmentVariable("FOUNDRY_LOCAL_BASE_URL") ?? DefaultBaseUrl;
var configuredModel = Environment.GetEnvironmentVariable("FOUNDRY_LOCAL_MODEL") ?? DefaultModel;
var apiKey = Environment.GetEnvironmentVariable("FOUNDRY_LOCAL_API_KEY") ?? DefaultApiKey;
var variantInput = args.FirstOrDefault()
    ?? Environment.GetEnvironmentVariable("FOUNDRY_LOCAL_PROMPT_VARIANT")
    ?? DefaultPromptVariant;
var baseUrl = NormalizeBaseUrl(configuredBaseUrl);

Console.WriteLine("Foundry Local streaming sample");
Console.WriteLine($"Endpoint: {baseUrl}");
Console.WriteLine();
Console.WriteLine("Prompt variants:");
Console.WriteLine("  1) eli5    - Explain like I'm 5");
Console.WriteLine("  2) bullets - Three tiny bullets");

var selectedPrompt = ResolvePromptVariant(variantInput);
if (selectedPrompt is null)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine($"Unknown prompt variant '{variantInput}'. Use: eli5 | bullets");
    return 1;
}

Console.WriteLine($"Selected variant: {selectedPrompt.Key}");

var preflight = await RunPreflightAsync(baseUrl, configuredModel);
if (!preflight.Ok)
{
    PrintGuidance(preflight.ErrorMessage!, configuredBaseUrl, configuredModel);
    return 1;
}

if (!string.Equals(preflight.SelectedModel, configuredModel, StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"Configured model '{configuredModel}' was not found. Using '{preflight.SelectedModel}' instead.");
}

var chatClient = new ChatClient(
    preflight.SelectedModel!,
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri(baseUrl) });

Console.WriteLine();
Console.WriteLine("Streaming response:");
var firstChunk = true;
await foreach (var update in chatClient.CompleteChatStreamingAsync(
[
    new SystemChatMessage("You are concise and presenter-friendly."),
    new UserChatMessage(selectedPrompt.Prompt)
]))
{
    foreach (var content in update.ContentUpdate)
    {
        if (string.IsNullOrEmpty(content.Text))
        {
            continue;
        }

        if (firstChunk)
        {
            Console.WriteLine();
            firstChunk = false;
        }

        Console.Write(content.Text);
    }
}

if (firstChunk)
{
    Console.WriteLine();
    Console.WriteLine("(empty response)");
}
else
{
    Console.WriteLine();
}

return 0;

static string NormalizeBaseUrl(string input)
{
    var normalized = input.Trim().TrimEnd('/');
    if (!normalized.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
    {
        normalized = $"{normalized}/v1";
    }

    return normalized;
}

static PromptVariant? ResolvePromptVariant(string input)
{
    var value = input.Trim().ToLowerInvariant();
    return value switch
    {
        "1" or "eli5" => new PromptVariant("eli5", "In one short paragraph, explain what a GPU does like I'm 5."),
        "2" or "bullets" => new PromptVariant("bullets", "Summarize why local AI inference is useful in exactly 3 short bullet points."),
        _ => null
    };
}

static async Task<PreflightResult> RunPreflightAsync(string baseUrl, string configuredModel)
{
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
    HttpResponseMessage response;

    try
    {
        response = await http.GetAsync($"{baseUrl}/models");
    }
    catch (Exception ex)
    {
        return PreflightResult.Fail($"Could not reach Foundry Local service at '{baseUrl}': {ex.Message}");
    }

    if (!response.IsSuccessStatusCode)
    {
        var body = await response.Content.ReadAsStringAsync();
        var snippet = string.IsNullOrWhiteSpace(body) ? "(no response body)" : body[..Math.Min(240, body.Length)];
        return PreflightResult.Fail($"Foundry Local service returned {(int)response.StatusCode} {response.ReasonPhrase}. Body: {snippet}");
    }

    var payload = await response.Content.ReadAsStringAsync();
    var modelIds = ExtractModelIds(payload);
    if (modelIds.Count == 0)
    {
        return PreflightResult.Fail("Foundry Local responded, but no models were listed at /v1/models.");
    }

    var selectedModel =
        modelIds.FirstOrDefault(id => string.Equals(id, configuredModel, StringComparison.OrdinalIgnoreCase))
        ?? modelIds[0];

    return PreflightResult.Success(selectedModel);
}

static List<string> ExtractModelIds(string payload)
{
    using var doc = JsonDocument.Parse(payload);
    if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
    {
        return [];
    }

    var models = new List<string>();
    foreach (var item in data.EnumerateArray())
    {
        if (item.TryGetProperty("id", out var idProp))
        {
            var id = idProp.GetString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                models.Add(id);
            }
        }
    }

    return models;
}

static void PrintGuidance(string reason, string configuredBaseUrl, string configuredModel)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("Preflight check failed.");
    Console.Error.WriteLine(reason);
    Console.Error.WriteLine();
    Console.Error.WriteLine("Try this:");
    Console.Error.WriteLine("  1) Verify Foundry Local CLI is installed: foundry --help");
    Console.Error.WriteLine("  2) Start or restart the local service: foundry service start");
    Console.Error.WriteLine("  3) Check status and endpoint: foundry service status");
    Console.Error.WriteLine($"  4) If needed, override endpoint: set FOUNDRY_LOCAL_BASE_URL={configuredBaseUrl}");
    Console.Error.WriteLine($"  5) If needed, override model: set FOUNDRY_LOCAL_MODEL={configuredModel}");
    Console.Error.WriteLine();
}

internal sealed record PreflightResult(bool Ok, string? SelectedModel, string? ErrorMessage)
{
    public static PreflightResult Success(string model) => new(true, model, null);
    public static PreflightResult Fail(string message) => new(false, null, message);
}

internal sealed record PromptVariant(string Key, string Prompt);
