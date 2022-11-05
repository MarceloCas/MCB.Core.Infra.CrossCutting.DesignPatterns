using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Observer;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Observer;

public abstract class PublisherBase
    : IPublisher
{
    // Fields
    private readonly Dictionary<Type, List<Type>> _subscriptionsDictionary;

    // Properties
    public Dictionary<Type, List<Type>> SubscriptionsDictionary => _subscriptionsDictionary.ToDictionary(entry => entry.Key, entry => entry.Value);

    // Constructors
    protected PublisherBase()
    {
        _subscriptionsDictionary = new Dictionary<Type, List<Type>>();
    }

    // Private Methods
    private void AddSubjectSubscriptionIfNotExists<TSubject>()
    {
        var subjectType = typeof(TSubject);

        if (_subscriptionsDictionary.ContainsKey(subjectType))
            return;

        _subscriptionsDictionary.Add(subjectType, new List<Type>());
    }
    private void AddSubscriptionIfNotExists<TSubscriber, TSubject>()
    {
        var subscriberType = typeof(TSubscriber);
        var subjectType = typeof(TSubject);

        AddSubjectSubscriptionIfNotExists<TSubject>();

        if (_subscriptionsDictionary[subjectType].Any(q => q == subscriberType))
            return;

        _subscriptionsDictionary[subjectType].Add(subscriberType);
    }
    private List<Type> GetSubscriberTypeCollection<TSubject>()
    {
        var subjectType = typeof(TSubject);

        if (!_subscriptionsDictionary.ContainsKey(subjectType))
            return new();

        return _subscriptionsDictionary[subjectType];
    }

    // Protected Abstract Methods
    protected abstract ISubscriber<TSubject> InstanciateSubscriber<TSubject>(Type subscriberType);

    // Public Methods
    public void Subscribe<TSubscriber, TSubject>() where TSubscriber : ISubscriber<TSubject>
    {
        AddSubscriptionIfNotExists<TSubscriber, TSubject>();
    }
    public async Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken)
    {
        var subscriberTypeCollection = GetSubscriberTypeCollection<TSubject>();

        foreach (var subscriberType in subscriberTypeCollection)
            await InstanciateSubscriber<TSubject>(subscriberType).HandlerAsync(subject, cancellationToken).ConfigureAwait(false);
    }
}
