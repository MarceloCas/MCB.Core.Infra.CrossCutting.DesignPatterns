using FluentAssertions;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Observer;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Observer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Tests.ObserverTests;

public class PublisherBaseTest
{
    [Fact]
    public async void Subject_Should_Puslished()
    {
        // Arrange
        var samplePublisher = new SamplePublisher();
        samplePublisher.Subscribe<SampleEventSubscriberA, SampleEvent>();
        samplePublisher.Subscribe<SampleEventSubscriberA, SampleEvent>();
        samplePublisher.Subscribe<SampleEventSubscriberB, SampleEvent>();

        var sampleEventGuid = Guid.NewGuid();
        var sampleEvent = new SampleEvent { Id = sampleEventGuid };

        // Act
        await samplePublisher.PublishAsync(sampleEvent, default).ConfigureAwait(false);
        await samplePublisher.PublishAsync(new NoSubscriberEvent(), default).ConfigureAwait(false);
        samplePublisher.SubscriptionsDictionary.Add(typeof(NoSubscriberEvent), new List<Type> { typeof(SampleEventSubscriberA) });

        // Assert
        SampleEventSubscriberA.ReceivedSubjects.Should().HaveCount(1);
        SampleEventSubscriberA.ReceivedSubjects[0].Id.Should().Be(sampleEventGuid);

        SampleEventSubscriberB.ReceivedSubjects.Should().HaveCount(1);
        SampleEventSubscriberB.ReceivedSubjects[0].Id.Should().Be(sampleEventGuid);

        samplePublisher.SubscriptionsDictionary.Count.Should().Be(1);
        samplePublisher.SubscriptionsDictionary[typeof(SampleEvent)].Count.Should().Be(2);
    }
}

public class SampleEvent
{
    public Guid Id { get; set; }
}
public class NoSubscriberEvent
{

}

public class SampleEventSubscriberA
    : ISubscriber<SampleEvent>
{
    // Properties
    public static List<SampleEvent> ReceivedSubjects { get; } = new();

    // Public Methods
    public Task HandlerAsync(SampleEvent subject, CancellationToken cancellationToken)
    {
        ReceivedSubjects.Add(subject);
        return Task.CompletedTask;
    }
}
public class SampleEventSubscriberB
    : ISubscriber<SampleEvent>
{
    // Properties
    public static List<SampleEvent> ReceivedSubjects { get; } = new();

    // Public Methods
    public Task HandlerAsync(SampleEvent subject, CancellationToken cancellationToken)
    {
        ReceivedSubjects.Add(subject);
        return Task.CompletedTask;
    }
}

public class SamplePublisher
    : PublisherBase
{
    protected override ISubscriber<TSubject> InstanciateSubscriber<TSubject>(Type subscriberType)
    {
        if (subscriberType is null)
            throw new InvalidOperationException();

        var instance = Activator.CreateInstance(subscriberType);
        if (instance is null)
            throw new InvalidOperationException();

        return (ISubscriber<TSubject>)instance;
    }
}
