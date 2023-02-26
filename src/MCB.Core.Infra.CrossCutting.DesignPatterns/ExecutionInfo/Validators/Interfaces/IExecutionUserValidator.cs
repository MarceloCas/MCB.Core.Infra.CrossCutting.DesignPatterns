using MCB.Core.Infra.CrossCutting.DesignPatterns.Validator.Abstractions;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.ExecutionInfo.Validators.Interfaces;
internal interface IExecutionUserValidator
    : IValidator<Abstractions.ExecutionInfo.ExecutionInfo>
{
    // TenantId
    static readonly string InputBaseShouldHaveTenantIdErrorCode = nameof(InputBaseShouldHaveTenantIdErrorCode);
    static readonly string InputBaseShouldHaveTenantIdMessage = nameof(InputBaseShouldHaveTenantIdMessage);
    static readonly FluentValidation.Severity InputBaseShouldHaveTenantIdSeverity = FluentValidation.Severity.Error;

    // ExecutionUser
    static readonly string InputBaseShouldHaveExecutionUserErrorCode = nameof(InputBaseShouldHaveExecutionUserErrorCode);
    static readonly string InputBaseShouldHaveExecutionUserMessage = nameof(InputBaseShouldHaveExecutionUserMessage);
    static readonly FluentValidation.Severity InputBaseShouldHaveExecutionUserSeverity = FluentValidation.Severity.Error;

    static readonly string InputBaseShouldHaveExecutionUserWithValidLengthErrorCode = nameof(InputBaseShouldHaveExecutionUserWithValidLengthErrorCode);
    static readonly string InputBaseShouldHaveExecutionUserWithValidLengthMessage = nameof(InputBaseShouldHaveExecutionUserWithValidLengthMessage);
    static readonly FluentValidation.Severity InputBaseShouldHaveExecutionUserWithValidLengthSeverity = FluentValidation.Severity.Error;

    // SourcePlatform
    static readonly string InputBaseShouldHaveSourcePlatformErrorCode = nameof(InputBaseShouldHaveSourcePlatformErrorCode);
    static readonly string InputBaseShouldHaveSourcePlatformMessage = nameof(InputBaseShouldHaveSourcePlatformMessage);
    static readonly FluentValidation.Severity InputBaseShouldHaveSourcePlatformSeverity = FluentValidation.Severity.Error;

    static readonly string InputBaseShouldHaveSourcePlatformWithValidLengthErrorCode = nameof(InputBaseShouldHaveSourcePlatformWithValidLengthErrorCode);
    static readonly string InputBaseShouldHaveSourcePlatformWithValidLengthMessage = nameof(InputBaseShouldHaveSourcePlatformWithValidLengthMessage);
    static readonly FluentValidation.Severity InputBaseShouldHaveSourcePlatformWithValidLengthSeverity = FluentValidation.Severity.Error;

    // CorrelationId
    static readonly string InputBaseShouldHaveCorrelationIdErrorCode = nameof(InputBaseShouldHaveCorrelationIdErrorCode);
    static readonly string InputBaseShouldHaveCorrelationIdMessage = nameof(InputBaseShouldHaveCorrelationIdMessage);
    static readonly FluentValidation.Severity InputBaseShouldHaveCorrelationIdSeverity = FluentValidation.Severity.Error;
}