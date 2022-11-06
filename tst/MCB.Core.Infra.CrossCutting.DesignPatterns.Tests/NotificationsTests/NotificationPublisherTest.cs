using FluentAssertions;
using Mapster;
using MCB.Core.Infra.CrossCutting.DependencyInjection;
using MCB.Core.Infra.CrossCutting.DependencyInjection.Abstractions.Enums;
using MCB.Core.Infra.CrossCutting.DependencyInjection.Abstractions.Interfaces;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Adapter;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Notifications;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Notifications.Models;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Notifications.Models.Enums;
using MCB.Core.Infra.CrossCutting.DesignPatterns.DependencyInjection;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Notifications;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Notifications.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Tests.NotificationsTests;

public class NotificationPublisherTest
{
    [Fact]
    public async Task NotificationPublisher_Should_PublishNotification()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        var dependencyInjectionContainer = new DependencyInjectionContainer(serviceCollection);
        Bootstrapper.ConfigureServices(
            dependencyInjectionContainer,
            adapterConfiguration =>
            {
                adapterConfiguration.DependencyInjectionLifecycle = DependencyInjectionLifecycle.Singleton;
                adapterConfiguration.TypeAdapterConfigurationFunction = new Func<TypeAdapterConfig>(() =>
                {
                    var typeAdapterConfig = new TypeAdapterConfig();

                    return typeAdapterConfig;
                });
            }
        );
        serviceCollection.AddSingleton<IDependencyInjectionContainer>(serviceProvider => dependencyInjectionContainer);
        dependencyInjectionContainer.Build();

        var notificationPublisher = dependencyInjectionContainer.Resolve<INotificationPublisher>()!;
        notificationPublisher.Should().NotBeNull();

        var notification = new Notification(
            NotificationType.Information,
            code: Guid.NewGuid().ToString(),
            description: Guid.NewGuid().ToString()
        );

        // Act
        await notificationPublisher.PublishNotificationAsync(
            notification,
            cancellationToken: default
        );
    }
}
