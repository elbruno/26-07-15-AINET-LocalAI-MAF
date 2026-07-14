using System.Text.Json;

namespace FoundryLocal.Samples.Common;

public static class FoundryLocalSampleSupport
{
    public const string BaseUrlEnvVar = "FOUNDRY_LOCAL_BASE_URL";
    public const string ModelEnvVar = "FOUNDRY_LOCAL_MODEL";
    public const string ApiKeyEnvVar = "FOUNDRY_LOCAL_API_KEY";

    public const string DefaultBaseUrl = "http://127.0.0.1:5273/v1";
    public const string DefaultModel = "qwen2.5-0.5b";
    public const string DefaultApiKey = "local-dev-key";

    public static FoundryLocalSampleSettings LoadSettings()
    {
        var configuredBaseUrl = Environment.GetEnvironmentVariable(BaseUrlEnvVar);
        var configuredModel = Environment.GetEnvironmentVariable(ModelEnvVar);
        var configuredApiKey = Environment.GetEnvironmentVariable(ApiKeyEnvVar);

        configuredBaseUrl = string.IsNullOrWhiteSpace(configuredBaseUrl) ? DefaultBaseUrl : configuredBaseUrl;
        configuredModel = string.IsNullOrWhiteSpace(configuredModel) ? DefaultModel : configuredModel;
        configuredApiKey = string.IsNullOrWhiteSpace(configuredApiKey) ? DefaultApiKey : configuredApiKey;

        return new FoundryLocalSampleSettings(
            NormalizeBaseUrl(configuredBaseUrl),
            configuredBaseUrl,
            configuredModel,
            configuredApiKey);
    }

    public static async Task<FoundryLocalPreflightResult> RunPreflightAsync(string baseUrl, string configuredModel, CancellationToken cancellationToken = default)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        HttpResponseMessage response;

        try
        {
            response = await http.GetAsync($"{baseUrl}/models", cancellationToken);
        }
        catch (Exception ex)
        {
            return FoundryLocalPreflightResult.Fail($"Could not reach Foundry Local service at '{baseUrl}': {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var snippet = string.IsNullOrWhiteSpace(body) ? "(no response body)" : body[..Math.Min(240, body.Length)];
            return FoundryLocalPreflightResult.Fail($"Foundry Local service returned {(int)response.StatusCode} {response.ReasonPhrase}. Body: {snippet}");
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var modelIds = ExtractModelIds(payload);
        if (modelIds.Count == 0)
        {
            return FoundryLocalPreflightResult.Fail("Foundry Local responded, but no models were listed at /v1/models.");
        }

        var selectedModel =
            modelIds.FirstOrDefault(id => string.Equals(id, configuredModel, StringComparison.OrdinalIgnoreCase))
            ?? modelIds[0];

        return FoundryLocalPreflightResult.Success(selectedModel);
    }

    public static string BuildPreflightGuidance(string reason, FoundryLocalSampleSettings settings)
    {
        return $"""
Preflight check failed.
{reason}

Try this:
  1) Verify Foundry Local CLI is installed: foundry --help
  2) Start or restart the local service: foundry service start
  3) Check status and endpoint: foundry service status
  4) If needed, override endpoint: set {BaseUrlEnvVar}={settings.ConfiguredBaseUrl}
  5) If needed, override model: set {ModelEnvVar}={settings.Model}
""";
    }

    private static string NormalizeBaseUrl(string input)
    {
        var normalized = input.Trim().TrimEnd('/');
        if (!normalized.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            normalized = $"{normalized}/v1";
        }

        return normalized;
    }

    private static List<string> ExtractModelIds(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var models = new List<string>();
            foreach (var item in data.EnumerateArray())
            {
                if (!item.TryGetProperty("id", out var idProp))
                {
                    continue;
                }

                var id = idProp.GetString();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    models.Add(id);
                }
            }

            return models;
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

public sealed record FoundryLocalSampleSettings(
    string BaseUrl,
    string ConfiguredBaseUrl,
    string Model,
    string ApiKey);

public sealed record FoundryLocalPreflightResult(bool Ok, string? SelectedModel, string? ErrorMessage)
{
    public static FoundryLocalPreflightResult Success(string model) => new(true, model, null);
    public static FoundryLocalPreflightResult Fail(string message) => new(false, null, message);
}
