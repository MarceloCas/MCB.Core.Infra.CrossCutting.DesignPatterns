using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Notifications;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Notifications.Models;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Notifications.Interfaces;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Notifications;

public class NotificationPublisher
    : INotificationPublisher
{
    // Fields
    private readonly INotificationPublisherInternal _notificationPublisherInternal;

    // Constructors
    public NotificationPublisher(INotificationPublisherInternal notificationPublisherInternal)
    {
        _notificationPublisherInternal = notificationPublisherInternal;
    }

    // Public Methods
    public Task PublishNotificationAsync(Notification notification, CancellationToken cancellationToken)
    {
        return _notificationPublisherInternal.PublishAsync(notification, cancellationToken);
    }
}
