using ElBruno.MAF.FoundryLocal;
using Microsoft.AI.Foundry.Local;
using System.Diagnostics;

namespace _02_aichatweb_local.Web.Services;

public sealed class FoundryLocalModelStatusService(
    string configuredModelAlias,
    FoundryLocalModelLifecycleService lifecycle)
{
    public FoundryLocalModelStatus GetCurrentStatus()
    {
        try
        {
            var snapshot = lifecycle.GetDiagnosticsSnapshot();
            var resolvedAlias = string.IsNullOrWhiteSpace(snapshot.ModelAlias)
                ? configuredModelAlias
                : snapshot.ModelAlias;

            return new FoundryLocalModelStatus(
                resolvedAlias,
                BuildStatusText(snapshot),
                snapshot.ModelLoaded);
        }
        catch
        {
            return new FoundryLocalModelStatus(configuredModelAlias, "Status unavailable", false);
        }
    }

    public async Task<FoundryLocalModelStatus> EnsureModelReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await lifecycle.GetChatClientAsync(cancellationToken);
            return GetCurrentStatus();
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            var statusText = ContainsAny(message, "not found", "missing", "download", "unavailable")
                ? "Unavailable (not downloaded)"
                : "Unavailable";
            return new FoundryLocalModelStatus(configuredModelAlias, statusText, false, message);
        }
    }

    public async Task<FoundryLocalModelActionResult> OpenModelLocationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var model = await ResolveModelAsync(cancellationToken);
            if (model is null)
            {
                return new FoundryLocalModelActionResult(false, $"Model '{configuredModelAlias}' was not found in catalog.");
            }

            var modelPath = await model.GetPathAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(modelPath) || !Directory.Exists(modelPath))
            {
                return new FoundryLocalModelActionResult(false, "Model is not downloaded yet.");
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = modelPath,
                UseShellExecute = true
            });

            return new FoundryLocalModelActionResult(true, $"Opened model folder: {modelPath}");
        }
        catch (Exception ex)
        {
            return new FoundryLocalModelActionResult(false, ex.Message);
        }
    }

    public async Task<FoundryLocalModelActionResult> DeleteDownloadedModelAsync(CancellationToken cancellationToken)
    {
        try
        {
            var model = await ResolveModelAsync(cancellationToken);
            if (model is null)
            {
                return new FoundryLocalModelActionResult(false, $"Model '{configuredModelAlias}' was not found in catalog.");
            }

            var modelPath = await model.GetPathAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(modelPath) || !Directory.Exists(modelPath))
            {
                return new FoundryLocalModelActionResult(false, "Model is not downloaded yet.");
            }

            await model.UnloadAsync(cancellationToken);
            await model.RemoveFromCacheAsync(cancellationToken);
            return new FoundryLocalModelActionResult(true, $"Deleted downloaded model cache for '{configuredModelAlias}'.");
        }
        catch (Exception ex)
        {
            return new FoundryLocalModelActionResult(false, ex.Message);
        }
    }

    private async Task<IModel?> ResolveModelAsync(CancellationToken cancellationToken)
    {
        var catalog = await FoundryLocalManager.Instance.GetCatalogAsync(cancellationToken);
        return await catalog.GetModelAsync(configuredModelAlias, cancellationToken);
    }

    private static string BuildStatusText(FoundryLocalDiagnosticsSnapshot snapshot)
    {
        if (snapshot.DownloadedThisSession && snapshot.ModelLoaded)
        {
            return "Loaded (downloaded this session)";
        }

        if (snapshot.ModelLoaded)
        {
            return "Loaded";
        }

        var warning = snapshot.Warnings.FirstOrDefault(static w => !string.IsNullOrWhiteSpace(w));
        if (!string.IsNullOrWhiteSpace(warning))
        {
            if (ContainsAny(warning, "not found", "missing", "download", "unavailable"))
            {
                return "Unavailable (not downloaded)";
            }

            return "Unavailable";
        }

        if (snapshot.DownloadedThisSession)
        {
            return "Downloaded this session (pending load)";
        }

        return "Not loaded yet";
    }

    private static bool ContainsAny(string value, params string[] terms)
        => terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
}

public sealed record FoundryLocalModelStatus(
    string ModelAlias,
    string StatusText,
    bool IsReady,
    string? Detail = null);

public sealed record FoundryLocalModelActionResult(bool Success, string Message);
