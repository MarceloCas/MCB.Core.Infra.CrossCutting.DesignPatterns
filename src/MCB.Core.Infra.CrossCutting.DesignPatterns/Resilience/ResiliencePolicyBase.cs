using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Resilience;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Resilience.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using CircuitState = MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Resilience.Enums.CircuitState;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Resilience;

public abstract class ResiliencePolicyBase
    : IResiliencePolicy
{
    // Constants
    private const int EXCEPTIONS_ALLOWED_BEFORE_BREAKING = 1;
    private const string RETRY_POLICY_CONTEXT_INPUT_KEY = "input";
    private const string RETRY_POLICY_CONTEXT_OUTPUT_KEY = "output";

    // Fields
    private AsyncRetryPolicy _asyncRetryPolicy;
    private AsyncCircuitBreakerPolicy _asyncCircuitBreakerPolicy;

    // Protected Properties
    protected ILogger Logger { get; }

    // Public Properties
    public string Name { get; private set; }

    public CircuitState CircuitState => GetCircuitState(_asyncCircuitBreakerPolicy.CircuitState);

    public int CurrentRetryCount { get; private set; }
    public int CurrentCircuitBreakerOpenCount { get; private set; }
    public ResiliencePolicyConfig ResiliencePolicyConfig { get; private set; }

    public ResiliencePolicyConfig ResilienceConfig => throw new NotImplementedException();

    // Constructors
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected ResiliencePolicyBase(ILogger logger)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Logger = logger;
        ResiliencePolicyConfig = new ResiliencePolicyConfig();

        ApplyConfig(ResiliencePolicyConfig);
        ResetCurrentCircuitBreakerOpenCount();
    }

    // Private Methods

    private void ResetCurrentRetryCount() => CurrentRetryCount = 0;
    private void IncrementRetryCount() => CurrentRetryCount++;
    private void ResetCurrentCircuitBreakerOpenCount() => CurrentCircuitBreakerOpenCount = 0;
    private void IncrementCircuitBreakerOpenCount() => CurrentCircuitBreakerOpenCount++;
    private void ConfigureRetryPolicy(ResiliencePolicyConfig resiliencePolicyConfig)
    {
        var retryPolicyBuilder = default(PolicyBuilder);
        foreach (var exceptionHandleConfig in resiliencePolicyConfig.ExceptionHandleConfigArray)
        {
            if (retryPolicyBuilder is null)
                retryPolicyBuilder = Policy.Handle(exceptionHandleConfig);
            else
                retryPolicyBuilder = retryPolicyBuilder.Or(exceptionHandleConfig);
        }

        _asyncRetryPolicy = retryPolicyBuilder.WaitAndRetryAsync(
            retryCount: resiliencePolicyConfig.RetryMaxAttemptCount,
            sleepDurationProvider: resiliencePolicyConfig.RetryAttemptWaitingTimeFunction,
            onRetry: (exception, retryAttemptWaitingTime) =>
            {
                IncrementRetryCount();

                resiliencePolicyConfig.OnRetryAditionalHandler?.Invoke((CurrentRetryCount, retryAttemptWaitingTime, exception));
            }
        );
    }
    private void ConfigureCircuitBreakerPolicy(ResiliencePolicyConfig resiliencePolicyConfig)
    {
        var circuitBreakerPolicyBuilder = default(PolicyBuilder);

        foreach (var exceptionHandleConfig in resiliencePolicyConfig.ExceptionHandleConfigArray)
        {
            if (circuitBreakerPolicyBuilder is null)
                circuitBreakerPolicyBuilder = Policy.Handle(exceptionHandleConfig);
            else
                circuitBreakerPolicyBuilder = circuitBreakerPolicyBuilder.Or(exceptionHandleConfig);
        }

        _asyncCircuitBreakerPolicy = circuitBreakerPolicyBuilder.CircuitBreakerAsync(
            exceptionsAllowedBeforeBreaking: EXCEPTIONS_ALLOWED_BEFORE_BREAKING,
            durationOfBreak: resiliencePolicyConfig.CircuitBreakerWaitingTimeFunction(),
            onBreak: (exception, waitingTime) =>
            {
                IncrementCircuitBreakerOpenCount();

                resiliencePolicyConfig.OnCircuitBreakerOpenAditionalHandler?.Invoke((CurrentCircuitBreakerOpenCount, waitingTime, exception));
            },
            onReset: () =>
            {
                ResetCurrentRetryCount();
                ResetCurrentCircuitBreakerOpenCount();

                resiliencePolicyConfig.OnCircuitBreakerCloseAditionalHandler?.Invoke();
            },
            onHalfOpen: () =>
            {
                ResetCurrentRetryCount();

                resiliencePolicyConfig.OnCircuitBreakerHalfOpenAditionalHandler?.Invoke();
            }
        );
    }
    private void ApplyConfig(ResiliencePolicyConfig resiliencePolicyConfig)
    {
        Name = resiliencePolicyConfig.Name;

        // Retry
        ConfigureRetryPolicy(resiliencePolicyConfig);

        // Circuit Breaker
        ConfigureCircuitBreakerPolicy(resiliencePolicyConfig);
    }

    // Protected Methods
    protected static CircuitState GetCircuitState(Polly.CircuitBreaker.CircuitState pollyCircuitState) =>
        pollyCircuitState switch
        {
            Polly.CircuitBreaker.CircuitState.Closed => CircuitState.Closed,
            Polly.CircuitBreaker.CircuitState.Open => CircuitState.Open,
            Polly.CircuitBreaker.CircuitState.HalfOpen => CircuitState.HalfOpen,
            Polly.CircuitBreaker.CircuitState.Isolated => CircuitState.Isolated,
            _ => throw new ArgumentOutOfRangeException(nameof(pollyCircuitState))
        };

    // Public Methods
    public void Configure(Func<ResiliencePolicyConfig> configureAction)
    {
        var resiliencePolicyConfig = configureAction();
        ResiliencePolicyConfig = resiliencePolicyConfig;
        ApplyConfig(ResiliencePolicyConfig);
    }
    public void CloseCircuitBreakerManually()
    {
        _asyncCircuitBreakerPolicy.Reset();
    }
    public void OpenCircuitBreakerManually()
    {
        _asyncCircuitBreakerPolicy.Isolate();
    }

    public async Task<bool> ExecuteAsync(Func<CancellationToken, Task> handler, CancellationToken cancellationToken)
    {
        var policyResult = await _asyncCircuitBreakerPolicy.ExecuteAndCaptureAsync(
            async (cancellationToken) =>
            {
                await _asyncRetryPolicy.ExecuteAsync(async () =>
                    await handler(cancellationToken).ConfigureAwait(false)
                ).ConfigureAwait(false);
            },
            cancellationToken
        ).ConfigureAwait(false);

        if (policyResult.Outcome != OutcomeType.Successful)
            return false;

        ResetCurrentRetryCount();
        return true;
    }
    public async Task<bool> ExecuteAsync<TInput>(Func<TInput?, CancellationToken, Task> handler, TInput? input, CancellationToken cancellationToken)
    {
        var policyResult = await _asyncCircuitBreakerPolicy.ExecuteAndCaptureAsync(
            async (context, cancellationToken) =>
            {
                await _asyncRetryPolicy.ExecuteAsync(async () =>
                    await handler(
                        (TInput?)context[RETRY_POLICY_CONTEXT_INPUT_KEY],
                        cancellationToken
                    ).ConfigureAwait(false)
                ).ConfigureAwait(false);
            },
            contextData: new Dictionary<string, object?> { { RETRY_POLICY_CONTEXT_INPUT_KEY, input } },
            cancellationToken
        ).ConfigureAwait(false);

        if (policyResult.Outcome != OutcomeType.Successful)
            return false;

        ResetCurrentRetryCount();
        return true;
    }
    public async Task<(bool success, TOutput? output)> ExecuteAsync<TInput, TOutput>(Func<TInput?, CancellationToken, Task<TOutput?>> handler, TInput? input, CancellationToken cancellationToken)
    {
        var policyResult = await _asyncCircuitBreakerPolicy.ExecuteAndCaptureAsync(
            async (context, cancellationToken) =>
            {
                context.Add(
                    key: RETRY_POLICY_CONTEXT_OUTPUT_KEY,
                    value: await _asyncRetryPolicy.ExecuteAsync(async () =>
                        await handler(
                            (TInput)context[RETRY_POLICY_CONTEXT_INPUT_KEY],
                            cancellationToken
                        ).ConfigureAwait(false)
                    ).ConfigureAwait(false)
                );
            },
            contextData: new Dictionary<string, object?> { { RETRY_POLICY_CONTEXT_INPUT_KEY, input } },
            cancellationToken
        ).ConfigureAwait(false);

        var success = policyResult.Outcome == OutcomeType.Successful;

        if (!success)
            return (success: false, output: default);

        ResetCurrentRetryCount();

        return (success, output: (TOutput)policyResult.Context[RETRY_POLICY_CONTEXT_OUTPUT_KEY]);
    }
}
