using Mapster;
using MCB.Core.Infra.CrossCutting.DependencyInjection.Abstractions.Enums;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.DependencyInjection.Models;

public class AdapterConfig
{
    // Properties
    public Func<TypeAdapterConfig>? TypeAdapterConfigurationFunction { get; set; }
    /// <summary>
    /// Default value: DependencyInjectionLifecycle.Singleton
    /// </summary>
    public DependencyInjectionLifecycle DependencyInjectionLifecycle { get; set; }

    public AdapterConfig()
    {
        DependencyInjectionLifecycle = DependencyInjectionLifecycle.Singleton;
    }
}
