using FluentAssertions;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Resilience.Enums;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Resilience.Models;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Resilience;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Tests.ResilienceTests;

[Collection(nameof(DefaultFixture))]
public class ResiliencePolicyBaseTest
{
    // Fields
    private readonly DefaultFixture _fixture;

    // Constructors
    public ResiliencePolicyBaseTest(
        DefaultFixture fixture
    )
    {
        _fixture = fixture;
    }

    // Private Methods
    private ResiliencePolicyWithAllConfig CreateResiliencePolicyWithAllConfig() => new(
        _fixture.ServiceProvider.GetService<ILogger<ResiliencePolicyWithAllConfig>>()
    );
    private ResiliencePolicyWithMinimumConfig CreateResiliencePolicyWithMinimumConfig() => new(
        _fixture.ServiceProvider.GetService<ILogger<ResiliencePolicyWithMinimumConfig>>()
    );

    [Fact]
    public void ResiliencePolicy_Should_Get_Correctly_Status()
    {
        // Arrange
        var pollyClosedStatus = Polly.CircuitBreaker.CircuitState.Closed;
        var pollyOpenStatus = Polly.CircuitBreaker.CircuitState.Open;
        var pollyHalfOpenStatus = Polly.CircuitBreaker.CircuitState.HalfOpen;
        var pollyIsolatedOpenStatus = Polly.CircuitBreaker.CircuitState.Isolated;
        var invalidPollyStatus = (Polly.CircuitBreaker.CircuitState)int.MaxValue;
        var hasErrorOnInvalidPollyStatus = false;

        // Act
        var closedStatus = ResiliencePolicyWithAllConfig.GetCircuitState(pollyClosedStatus);
        var openStatus = ResiliencePolicyWithAllConfig.GetCircuitState(pollyOpenStatus);
        var halfOpenStatus = ResiliencePolicyWithAllConfig.GetCircuitState(pollyHalfOpenStatus);
        var isolatedOpenStatus = ResiliencePolicyWithAllConfig.GetCircuitState(pollyIsolatedOpenStatus);
        try
        {
            ResiliencePolicyWithAllConfig.GetCircuitState(invalidPollyStatus);
        }
        catch (ArgumentOutOfRangeException)
        {
            hasErrorOnInvalidPollyStatus = true;
        }

        // Assert
        closedStatus.Should().Be(CircuitState.Closed);
        openStatus.Should().Be(CircuitState.Open);
        halfOpenStatus.Should().Be(CircuitState.HalfOpen);
        isolatedOpenStatus.Should().Be(CircuitState.Isolated);
        hasErrorOnInvalidPollyStatus.Should().BeTrue();
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Execute_With_Success()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var resiliencePolicyWithMinimumConfig = CreateResiliencePolicyWithMinimumConfig();
        var successOnRunResiliencePolicyWithAllConfig = false;
        var successOnRunResiliencePolicyWithMinimumConfig = false;

        // Act
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );
        successOnRunResiliencePolicyWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );

        // Assert
        successOnRunResiliencePolicyWithAllConfig.Should().BeTrue();
        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(0);

        successOnRunResiliencePolicyWithMinimumConfig.Should().BeTrue();
        resiliencePolicyWithMinimumConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithMinimumConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithMinimumConfig.CurrentRetryCount.Should().Be(0);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Fail()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var resiliencePolicyWithMinimumConfig = CreateResiliencePolicyWithMinimumConfig();
        var successOnRunResiliencePolicyWithAllConfig = false;
        var successOnRunResiliencePolicyWithMinimumConfig = false;

        // Act
        var stopwatch = Stopwatch.StartNew();
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                throw new ArgumentException();
            },
            cancellationToken: default
        );

        successOnRunResiliencePolicyWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                throw new ArgumentException();
            },
            cancellationToken: default
        );
        stopwatch.Stop();

        // Assert
        successOnRunResiliencePolicyWithAllConfig.Should().BeFalse();
        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Open);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(1);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);

        resiliencePolicyWithMinimumConfig.CircuitState.Should().Be(CircuitState.Open);
        successOnRunResiliencePolicyWithMinimumConfig.Should().BeFalse();
        resiliencePolicyWithMinimumConfig.CurrentCircuitBreakerOpenCount.Should().Be(1);
        resiliencePolicyWithMinimumConfig.CurrentRetryCount.Should().Be(resiliencePolicyWithMinimumConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Not_Execute_Any_Policy_When_Throw_Exception_Not_Handled()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var successOnRunResiliencePolicyWithAllConfig = false;

        // Act
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                throw new Exception();
            },
            cancellationToken: default
        );

        // Assert
        successOnRunResiliencePolicyWithAllConfig.Should().BeFalse();
        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(0);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Apply_Retry_Policy()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var successOnRunResiliencePolicyWithAllConfig = false;

        // Act
        var stopwatch = Stopwatch.StartNew();
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                if (resiliencePolicyWithAllConfig.CurrentRetryCount < resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount)
                    throw new InvalidOperationException();

                return Task.CompletedTask;
            },
            cancellationToken: default
        );
        stopwatch.Stop();

        // Assert
        successOnRunResiliencePolicyWithAllConfig.Should().BeTrue();
        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Be_HalfOpen_CircuitStatus()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var resiliencePolicyWithMinimumConfig = CreateResiliencePolicyWithMinimumConfig();
        var successOnRunResiliencePolicyWithAllConfig = false;
        var successOnRunResiliencePolicyWithMinimumlConfig = false;

        // Act
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                throw new ArgumentException();
            },
            cancellationToken: default
        );
        await Task.Delay(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));

        successOnRunResiliencePolicyWithMinimumlConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                throw new ArgumentException();
            },
            cancellationToken: default
        );
        await Task.Delay(resiliencePolicyWithMinimumConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));

        // Assert
        successOnRunResiliencePolicyWithAllConfig.Should().BeFalse();
        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.HalfOpen);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(1);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);

        successOnRunResiliencePolicyWithMinimumlConfig.Should().BeFalse();
        resiliencePolicyWithMinimumConfig.CircuitState.Should().Be(CircuitState.HalfOpen);
        resiliencePolicyWithMinimumConfig.CurrentCircuitBreakerOpenCount.Should().Be(1);
        resiliencePolicyWithMinimumConfig.CurrentRetryCount.Should().Be(0);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Be_Closed_After_Immediately_Success_During_HalfOpen_CircuitStatus()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var resiliencePolicyWithMinimumConfig = CreateResiliencePolicyWithMinimumConfig();
        var successOnRunResiliencePolicyWithAllConfig = false;
        var successOnRunResiliencePolicyWithMinimumConfig = false;

        // Act
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                throw new ArgumentException();
            },
            cancellationToken: default
        );
        await Task.Delay(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );

        successOnRunResiliencePolicyWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                throw new ArgumentException();
            },
            cancellationToken: default
        );
        await Task.Delay(resiliencePolicyWithMinimumConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));
        successOnRunResiliencePolicyWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );

        // Assert
        successOnRunResiliencePolicyWithAllConfig.Should().BeTrue();
        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);

        successOnRunResiliencePolicyWithMinimumConfig.Should().BeTrue();
        resiliencePolicyWithMinimumConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithMinimumConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithMinimumConfig.CurrentRetryCount.Should().Be(0);
    }
    [Fact]
    public async Task ResiliencePolicy_Should_Be_Closed_After_Success_During_HalfOpen_CircuitStatus()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var resiliencePolicyWithMinimumConfig = CreateResiliencePolicyWithMinimumConfig();
        var successOnRunResiliencePolicyWithAllConfig = false;
        var successOnRunResiliencePolicyWithMinimumConfig = false;

        // Act
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                throw new ArgumentException();
            },
            cancellationToken: default
        );
        await Task.Delay(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                if (resiliencePolicyWithAllConfig.CurrentRetryCount < resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount)
                    throw new InvalidOperationException();

                return Task.CompletedTask;
            },
            cancellationToken: default
        );

        successOnRunResiliencePolicyWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                throw new ArgumentException();
            },
            cancellationToken: default
        );
        await Task.Delay(resiliencePolicyWithMinimumConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));
        successOnRunResiliencePolicyWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                if (resiliencePolicyWithMinimumConfig.CurrentRetryCount < resiliencePolicyWithMinimumConfig.ResiliencePolicyConfig.RetryMaxAttemptCount)
                    throw new InvalidOperationException();

                return Task.CompletedTask;
            },
            cancellationToken: default
        );

        // Assert
        successOnRunResiliencePolicyWithAllConfig.Should().BeTrue();
        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount * 2);

        successOnRunResiliencePolicyWithMinimumConfig.Should().BeTrue();
        resiliencePolicyWithMinimumConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithMinimumConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithMinimumConfig.CurrentRetryCount.Should().Be(0);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Be_Opened_Manually()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var resiliencePolicyWithMinimumConfig = CreateResiliencePolicyWithMinimumConfig();
        var successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );
        var successOnRunResiliencePolicyWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );
        var successOnRunResiliencePolicyAfterManuallyOpenedWithAllConfig = false;
        var successOnRunResiliencePolicyAfterManuallyOpenedWithMinimumConfig = false;

        // Act
        resiliencePolicyWithAllConfig.OpenCircuitBreakerManually();
        resiliencePolicyWithMinimumConfig.OpenCircuitBreakerManually();

        await Task.Delay(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));
        await Task.Delay(resiliencePolicyWithMinimumConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));

        successOnRunResiliencePolicyAfterManuallyOpenedWithAllConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );
        successOnRunResiliencePolicyAfterManuallyOpenedWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );

        // Assert
        successOnRunResiliencePolicyWithAllConfig.Should().BeTrue();
        successOnRunResiliencePolicyWithMinimumConfig.Should().BeTrue();
        successOnRunResiliencePolicyAfterManuallyOpenedWithAllConfig.Should().BeFalse();
        successOnRunResiliencePolicyAfterManuallyOpenedWithMinimumConfig.Should().BeFalse();

        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Isolated);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(1);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(0);

        resiliencePolicyWithMinimumConfig.CircuitState.Should().Be(CircuitState.Isolated);
        resiliencePolicyWithMinimumConfig.CurrentCircuitBreakerOpenCount.Should().Be(1);
        resiliencePolicyWithMinimumConfig.CurrentRetryCount.Should().Be(0);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Be_Closed_Manually()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var resiliencePolicyWithMinimumConfig = CreateResiliencePolicyWithMinimumConfig();

        resiliencePolicyWithAllConfig.OpenCircuitBreakerManually();
        resiliencePolicyWithMinimumConfig.OpenCircuitBreakerManually();

        await Task.Delay(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));
        await Task.Delay(resiliencePolicyWithMinimumConfig.ResiliencePolicyConfig.CircuitBreakerWaitingTimeFunction().Add(TimeSpan.FromSeconds(1)));

        var successOnRunResiliencePolicyAfterOpenAndWaitCircuitBreakerWaitingTimeWithAllConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );
        var successOnRunResiliencePolicyAfterOpenAndWaitCircuitBreakerWaitingTimeWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );

        // Act
        resiliencePolicyWithAllConfig.CloseCircuitBreakerManually();
        resiliencePolicyWithMinimumConfig.CloseCircuitBreakerManually();

        var successOnRunResiliencePolicyAfterManuallyClosedWithAllConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );
        var successOnRunResiliencePolicyAfterManuallyClosedWithMinimumConfig = await resiliencePolicyWithMinimumConfig.ExecuteAsync(
            (cancellationToken) =>
            {
                return Task.CompletedTask;
            },
            cancellationToken: default
        );

        // Assert
        successOnRunResiliencePolicyAfterOpenAndWaitCircuitBreakerWaitingTimeWithAllConfig.Should().BeFalse();
        successOnRunResiliencePolicyAfterOpenAndWaitCircuitBreakerWaitingTimeWithMinimumConfig.Should().BeFalse();
        successOnRunResiliencePolicyAfterManuallyClosedWithAllConfig.Should().BeTrue();
        successOnRunResiliencePolicyAfterManuallyClosedWithMinimumConfig.Should().BeTrue();

        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(0);

        resiliencePolicyWithMinimumConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithMinimumConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithMinimumConfig.CurrentRetryCount.Should().Be(0);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Pass_Input()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();
        var successOnRunResiliencePolicyWithAllConfig = false;

        var id = Guid.NewGuid();
        var name = "Marcelo Castelo Branco";
        var inputIsValid = false;

        // Act
        successOnRunResiliencePolicyWithAllConfig = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (input, cancellationToken) =>
            {

                inputIsValid = input.Id == id && input.Name == name;

                return Task.CompletedTask;
            },
            input: new
            {
                Id = id,
                Name = name
            },
            cancellationToken: default
        );

        // Assert
        inputIsValid.Should().BeTrue();
        successOnRunResiliencePolicyWithAllConfig.Should().BeTrue();
        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(0);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Pass_Input_With_Error()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();

        var id = Guid.NewGuid();
        var name = "Marcelo Castelo Branco";
        var expectedOutput = $"{id}-{name}";

        // Act
        var success = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (input, cancellationToken) =>
            {
                throw new ArgumentException();
            },
            input: (id, name),
            cancellationToken: default
        );

        // Assert
        success.Should().BeFalse();

        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Open);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(1);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Pass_Input_And_Return_Output()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();

        var id = Guid.NewGuid();
        var name = "Marcelo Castelo Branco";
        var expectedOutput = $"{id}-{name}";

        // Act
        var (success, output) = await resiliencePolicyWithAllConfig.ExecuteAsync(
            (input, cancellationToken) =>
            {
                return Task.FromResult($"{input.id}-{input.name}");
            },
            input: (id, name),
            cancellationToken: default
        );

        // Assert
        success.Should().BeTrue();
        output.Should().Be(expectedOutput);
        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Closed);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(0);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(0);
    }

    [Fact]
    public async Task ResiliencePolicy_Should_Pass_Input_And_Return_Output_With_Error()
    {
        // Arrange
        var resiliencePolicyWithAllConfig = CreateResiliencePolicyWithAllConfig();

        var id = Guid.NewGuid();
        var name = "Marcelo Castelo Branco";
        var expectedOutput = $"{id}-{name}";

        // Act
        var (success, output) = await resiliencePolicyWithAllConfig.ExecuteAsync<(Guid id, string name), string>(
            (input, cancellationToken) =>
            {
                throw new ArgumentException();
            },
            input: (id, name),
            cancellationToken: default
        );

        // Assert
        success.Should().BeFalse();
        output.Should().BeNull();

        resiliencePolicyWithAllConfig.CircuitState.Should().Be(CircuitState.Open);
        resiliencePolicyWithAllConfig.CurrentCircuitBreakerOpenCount.Should().Be(1);
        resiliencePolicyWithAllConfig.CurrentRetryCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);
        resiliencePolicyWithAllConfig.OnCircuitBreakerHalfOpenAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnCircuitBreakerOpenAditionalHandlerCount.Should().Be(1);
        resiliencePolicyWithAllConfig.OnCircuitBreakerCloseAditionalHandlerCount.Should().Be(0);
        resiliencePolicyWithAllConfig.OnRetryAditionalHandlerCount.Should().Be(resiliencePolicyWithAllConfig.ResiliencePolicyConfig.RetryMaxAttemptCount);
    }
}

public class ResiliencePolicyWithAllConfig
    : ResiliencePolicyBase
{
    // Properties
    public int OnRetryAditionalHandlerCount { get; private set; }
    public int OnCircuitBreakerHalfOpenAditionalHandlerCount { get; private set; }
    public int OnCircuitBreakerOpenAditionalHandlerCount { get; private set; }
    public int OnCircuitBreakerCloseAditionalHandlerCount { get; private set; }

    // Constructors
    public ResiliencePolicyWithAllConfig(ILogger<ResiliencePolicyWithAllConfig> logger)
        : base(logger)
    {
        Configure(() =>
        {
            return new ResiliencePolicyConfig
            {
                // Identification
                Name = nameof(ResiliencePolicyWithAllConfig),
                // Retry
                RetryMaxAttemptCount = 5,
                RetryAttemptWaitingTimeFunction = (attempt) => TimeSpan.FromMilliseconds(100 * attempt),
                OnRetryAditionalHandler = ((int currentRetryCount, TimeSpan retryAttemptWaitingTime, Exception exception) input) =>
                {
                    OnRetryAditionalHandlerCount++;
                },
                // Circuit Breaker
                CircuitBreakerWaitingTimeFunction = () => TimeSpan.FromSeconds(3),
                OnCircuitBreakerHalfOpenAditionalHandler = () => { OnCircuitBreakerHalfOpenAditionalHandlerCount++; },
                OnCircuitBreakerOpenAditionalHandler = ((int currentCircuitBreakerOpenCount, TimeSpan circuitBreakerWaitingTime, Exception exception) input) =>
                {
                    OnCircuitBreakerOpenAditionalHandlerCount++;
                },
                OnCircuitBreakerCloseAditionalHandler = () => { OnCircuitBreakerCloseAditionalHandlerCount++; },
                // Exceptions
                ExceptionHandleConfigArray = new[] {
                    new Func<Exception, bool>(ex => ex.GetType() == typeof(ArgumentException)),
                    new Func<Exception, bool>(ex => ex.GetType() == typeof(InvalidOperationException))
                }
            };
        });
    }

    // Public Methods
    public static new CircuitState GetCircuitState(Polly.CircuitBreaker.CircuitState pollyCircuitState) => ResiliencePolicyBase.GetCircuitState(pollyCircuitState);
}
public class ResiliencePolicyWithMinimumConfig
    : ResiliencePolicyBase
{
    // Constructors
    public ResiliencePolicyWithMinimumConfig(ILogger<ResiliencePolicyWithMinimumConfig> logger)
        : base(logger)
    {
        Configure(() =>
        {
            return new ResiliencePolicyConfig
            {
                // Identification
                Name = nameof(ResiliencePolicyWithMinimumConfig),
                // Retry
                RetryMaxAttemptCount = 5,
                RetryAttemptWaitingTimeFunction = (attempt) => TimeSpan.FromMilliseconds(100 * attempt),
                // Circuit Breaker
                CircuitBreakerWaitingTimeFunction = () => TimeSpan.FromSeconds(3),
                // Exceptions
                ExceptionHandleConfigArray = new[] {
                    new Func<Exception, bool>(ex => ex.GetType() == typeof(ArgumentException)),
                    new Func<Exception, bool>(ex => ex.GetType() == typeof(InvalidOperationException))
                }
            };
        });
    }

    // Public Methods
    public static new CircuitState GetCircuitState(Polly.CircuitBreaker.CircuitState pollyCircuitState) => ResiliencePolicyBase.GetCircuitState(pollyCircuitState);
}
