using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Observer;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Observer;

namespace Bechmarks.Benchs.ObserverBenchs;

[SimpleJob(RunStrategy.Throughput, launchCount: 1)]
[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
[MemoryDiagnoser]
[HtmlExporter]
public class ObserverBench
{
    [Params(1, 3, 5, 10, 15)]
    public int IterationCount { get; set; }

    private static SamplePublisher CreateSamplePublisher()
    {
        var samplePublisher = new SamplePublisher();
        samplePublisher.Subscribe<SampleEventSubscriberA, SampleEvent>();
        samplePublisher.Subscribe<SampleEventSubscriberA, SampleEvent>();
        samplePublisher.Subscribe<SampleEventSubscriberB, SampleEvent>();

        return samplePublisher;
    }

    [Benchmark(Baseline = true)]
    public async Task Publish_WithFiveSubscribers_SinglePublisherInstance()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var samplePublisher = CreateSamplePublisher();

        var sampleEvent = new SampleEvent();

        for (int i = 0; i < IterationCount; i++)
            await samplePublisher.PublishAsync(sampleEvent, cancellationToken).ConfigureAwait(false);
    }
    [Benchmark]
    public async Task Publish_WithFiveSubscribers_MultiPublisherInstance()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var sampleEvent = new SampleEvent();

        for (int i = 0; i < IterationCount; i++)
        {
            var samplePublisher = CreateSamplePublisher();
            await samplePublisher.PublishAsync(sampleEvent, cancellationToken).ConfigureAwait(false);
        }
    }


    [Benchmark]
    public void Publish_WithFiveSubscribers_SinglePublisherInstance_Parallel()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var samplePublisher = CreateSamplePublisher();

        var sampleEvent = new SampleEvent();

        Parallel.For(0, IterationCount, async i => {
            await samplePublisher.PublishAsync(sampleEvent, cancellationToken).ConfigureAwait(false);
        });
    }
    [Benchmark]
    public void Publish_WithFiveSubscribers_MultiPublisherInstance_Parallel()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var sampleEvent = new SampleEvent();

        Parallel.For(0, IterationCount, async i => {
            var samplePublisher = CreateSamplePublisher();
            await samplePublisher.PublishAsync(sampleEvent, cancellationToken).ConfigureAwait(false);
        });
    }

    public class SampleEvent
    {
        public Guid Id { get; set; }
    }

    public class SampleEventSubscriberA
        : ISubscriber<SampleEvent>
    {
        // Public Methods
        public Task HandlerAsync(SampleEvent subject, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    public class SampleEventSubscriberB
        : ISubscriber<SampleEvent>
    {
        // Public Methods
        public Task HandlerAsync(SampleEvent subject, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    public class SampleEventSubscriberC
        : ISubscriber<SampleEvent>
    {
        // Public Methods
        public Task HandlerAsync(SampleEvent subject, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    public class SampleEventSubscriberD
        : ISubscriber<SampleEvent>
    {
        // Public Methods
        public Task HandlerAsync(SampleEvent subject, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    public class SampleEventSubscriberE
        : ISubscriber<SampleEvent>
    {
        // Public Methods
        public Task HandlerAsync(SampleEvent subject, CancellationToken cancellationToken)
        {
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
}
