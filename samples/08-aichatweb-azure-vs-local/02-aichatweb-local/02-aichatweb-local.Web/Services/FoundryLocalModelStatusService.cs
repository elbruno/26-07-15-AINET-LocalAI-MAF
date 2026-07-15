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
                snapshot.ModelLoaded,
                snapshot.DownloadedThisSession,
                true);
        }
        catch
        {
            return new FoundryLocalModelStatus(configuredModelAlias, "Status unavailable", false, false, false);
        }
    }

    public async Task<FoundryLocalModelStatus> GetCurrentStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = lifecycle.GetDiagnosticsSnapshot();
            var resolvedAlias = string.IsNullOrWhiteSpace(snapshot.ModelAlias)
                ? configuredModelAlias
                : snapshot.ModelAlias;

            var model = await ResolveModelAsync(cancellationToken);
            if (model is null)
            {
                return new FoundryLocalModelStatus(
                    resolvedAlias,
                    "Unavailable (model not in catalog)",
                    false,
                    false,
                    false,
                    null,
                    $"Model '{resolvedAlias}' was not found in the local Foundry catalog.");
            }

            var isDownloaded = await model.IsCachedAsync(cancellationToken);
            var isLoaded = await model.IsLoadedAsync(cancellationToken);
            var modelPath = isDownloaded ? await model.GetPathAsync(cancellationToken) : null;
            var statusText = BuildStatusText(snapshot, isDownloaded, isLoaded);
            var detail = BuildDetailText(snapshot, isDownloaded, isLoaded);

            return new FoundryLocalModelStatus(
                resolvedAlias,
                statusText,
                isLoaded,
                isDownloaded,
                true,
                modelPath,
                detail);
        }
        catch (Exception ex)
        {
            return new FoundryLocalModelStatus(configuredModelAlias, "Status unavailable", false, false, false, null, ex.Message);
        }
    }

    public async Task<FoundryLocalModelStatus> EnsureModelReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await lifecycle.DownloadModelAsync(configuredModelAlias, cancellationToken);

            var model = await ResolveModelAsync(cancellationToken);
            if (model is null)
            {
                return new FoundryLocalModelStatus(
                    configuredModelAlias,
                    "Unavailable (model not in catalog)",
                    false,
                    false,
                    false,
                    null,
                    $"Model '{configuredModelAlias}' was not found in the local Foundry catalog.");
            }

            await model.LoadAsync(cancellationToken);
            await lifecycle.GetChatClientAsync(cancellationToken);
            return await GetCurrentStatusAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            var statusText = ContainsAny(message, "not found", "missing", "download", "unavailable")
                ? "Unavailable (not downloaded)"
                : "Unavailable";
            return new FoundryLocalModelStatus(configuredModelAlias, statusText, false, false, true, null, message);
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
            var isDownloaded = await model.IsCachedAsync(cancellationToken);
            if (!isDownloaded || string.IsNullOrWhiteSpace(modelPath) || !Directory.Exists(modelPath))
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
            var isDownloaded = await model.IsCachedAsync(cancellationToken);
            if (!isDownloaded || string.IsNullOrWhiteSpace(modelPath) || !Directory.Exists(modelPath))
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

    private static string BuildStatusText(FoundryLocalDiagnosticsSnapshot snapshot, bool isDownloaded, bool isLoaded)
    {
        if (isLoaded)
        {
            return snapshot.DownloadedThisSession
                ? "Loaded (downloaded this session)"
                : "Loaded";
        }

        if (isDownloaded)
        {
            return "Downloaded (not loaded)";
        }

        return "Not downloaded";
    }

    private static string? BuildDetailText(FoundryLocalDiagnosticsSnapshot snapshot, bool isDownloaded, bool isLoaded)
    {
        var warning = snapshot.Warnings.FirstOrDefault(static w => !string.IsNullOrWhiteSpace(w));
        if (!string.IsNullOrWhiteSpace(warning))
        {
            return warning;
        }

        if (isLoaded)
        {
            return "Model is loaded and ready for chat.";
        }

        return isDownloaded
            ? "Use Prepare model to load the cached model into memory."
            : "Use Prepare model to download and load this model.";
    }

    private static bool ContainsAny(string value, params string[] terms)
        => terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
}

public sealed record FoundryLocalModelStatus(
    string ModelAlias,
    string StatusText,
    bool IsReady,
    bool IsDownloaded,
    bool IsInCatalog,
    string? ModelPath = null,
    string? Detail = null);

public sealed record FoundryLocalModelActionResult(bool Success, string Message);
