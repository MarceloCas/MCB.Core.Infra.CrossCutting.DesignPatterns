using FluentAssertions;
using Mapster;
using MCB.Core.Infra.CrossCutting.DependencyInjection;
using MCB.Core.Infra.CrossCutting.DependencyInjection.Abstractions.Enums;
using MCB.Core.Infra.CrossCutting.DependencyInjection.Abstractions.Interfaces;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Notifications;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Notifications.Models;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Notifications.Models.Enums;
using MCB.Core.Infra.CrossCutting.DesignPatterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
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
        var notificationSubscriber = dependencyInjectionContainer.Resolve<INotificationSubscriber>()!;

        notificationPublisher.Should().NotBeNull();

        var notificationA = new Notification(
            NotificationType.Information,
            code: Guid.NewGuid().ToString(),
            description: Guid.NewGuid().ToString()
        );
        var notificationB = new Notification(
            NotificationType.Warning,
            code: Guid.NewGuid().ToString(),
            description: Guid.NewGuid().ToString()
        );
        var notificationC = new Notification(
            NotificationType.Error,
            code: Guid.NewGuid().ToString(),
            description: Guid.NewGuid().ToString()
        );

        // Act
        await notificationPublisher.PublishNotificationAsync(
            notificationA,
            cancellationToken: default
        );
        await notificationPublisher.PublishNotificationAsync(
            notificationB,
            cancellationToken: default
        );
        await notificationPublisher.PublishNotificationAsync(
            notificationC,
            cancellationToken: default
        );

        // Assert
        notificationSubscriber.NotificationCollection.Should().HaveCount(3);

        notificationSubscriber.NotificationCollection.First(q => q.NotificationType == NotificationType.Information).Should().Be(notificationA);
        notificationSubscriber.NotificationCollection.First(q => q.NotificationType == NotificationType.Warning).Should().Be(notificationB);
        notificationSubscriber.NotificationCollection.First(q => q.NotificationType == NotificationType.Error).Should().Be(notificationC);
    }

    [Fact]
    public async Task NotificationPublisher_Should_Not_Publish_Unregistered_Subject()
    {
        // Arrange
        var hasRaisedError = false;
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
        dependencyInjectionContainer.Unregister<INotificationSubscriber>();

        serviceCollection.AddSingleton<IDependencyInjectionContainer>(serviceProvider => dependencyInjectionContainer);
        dependencyInjectionContainer.Build();

        var notificationPublisher = dependencyInjectionContainer.Resolve<INotificationPublisher>()!;
        var notificationSubscriber = dependencyInjectionContainer.Resolve<INotificationSubscriber>()!;

        notificationPublisher.Should().NotBeNull();

        // Act
        try
        {
            await notificationPublisher.PublishNotificationAsync(
                new Notification(NotificationType.Information, code: Guid.NewGuid().ToString(), description: Guid.NewGuid().ToString()),
                cancellationToken: default
            );
        }
        catch (InvalidOperationException)
        {
            hasRaisedError = true;
        }

        // Assert
        hasRaisedError.Should().BeTrue();
    }
}
