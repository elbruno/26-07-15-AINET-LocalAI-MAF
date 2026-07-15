using ElBruno.MAF.FoundryLocal;
using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MAF.FoundryLocal.Components;

/// <summary>
/// Registration helpers for the Foundry Local Blazor components.
/// </summary>
public static class FoundryLocalComponentsServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="FoundryLocalModelStatusService"/> for the configured model alias.
    /// The <see cref="FoundryLocalModelLifecycleService"/> must already be registered in the
    /// container (it owns the single <c>FoundryLocalManager</c> instance shared with the status
    /// service).
    /// </summary>
    public static IServiceCollection AddFoundryLocalComponents(
        this IServiceCollection services,
        string modelAlias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelAlias);

        services.AddSingleton(sp => new FoundryLocalModelStatusService(
            modelAlias,
            sp.GetRequiredService<FoundryLocalModelLifecycleService>()));

        return services;
    }
}
