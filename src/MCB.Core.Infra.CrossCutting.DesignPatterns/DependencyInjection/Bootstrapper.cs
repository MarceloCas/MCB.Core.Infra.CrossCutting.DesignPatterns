using Mapster;
using MapsterMapper;
using MCB.Core.Infra.CrossCutting.DependencyInjection.Abstractions.Interfaces;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Adapter;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Notifications;
using MCB.Core.Infra.CrossCutting.DesignPatterns.DependencyInjection.Models;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Notifications;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Notifications.Interfaces;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.DependencyInjection;

public static class Bootstrapper
{
    // Public Static Methods
    public static void ConfigureServices(
        IDependencyInjectionContainer dependencyInjectionContainer,
        Action<AdapterConfig> adapterConfigurationAction
    )
    {
        ConfigureServicesForAdapterPattern(dependencyInjectionContainer, adapterConfigurationAction);
        ConfigureServicesForNotifications(dependencyInjectionContainer);
    }

    // Private Static Methods
    private static void ConfigureServicesForAdapterPattern(IDependencyInjectionContainer dependencyInjectionContainer, Action<AdapterConfig> adapterConfigurationAction)
    {
        if (adapterConfigurationAction is null)
            return;

        var adapterConfig = new AdapterConfig();
        adapterConfigurationAction(adapterConfig);

        dependencyInjectionContainer.Register(
            lifecycle: adapterConfig.DependencyInjectionLifecycle,
            serviceType: typeof(IMapper),
            serviceTypeFactory: dependencyInjectionContainer => new Mapper(adapterConfig.TypeAdapterConfigurationFunction?.Invoke() ?? new TypeAdapterConfig())
        );

        dependencyInjectionContainer.Register(
            lifecycle: adapterConfig.DependencyInjectionLifecycle,
            serviceType: typeof(IAdapter),
            concreteType: typeof(Adapter.Adapter)
        );
    }
    private static void ConfigureServicesForNotifications(IDependencyInjectionContainer dependencyInjectionContainer)
    {
        dependencyInjectionContainer.RegisterScoped<INotificationPublisherInternal, NotificationPublisherInternal>();
        dependencyInjectionContainer.RegisterScoped<INotificationPublisher, NotificationPublisher>();
        dependencyInjectionContainer.RegisterScoped<INotificationSubscriber, NotificationSubscriber>();
    }
}
