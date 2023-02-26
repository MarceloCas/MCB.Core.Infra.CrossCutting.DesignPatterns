using FluentValidation;
using MCB.Core.Infra.CrossCutting.DesignPatterns.ExecutionInfo.Specifications.Interfaces;
using MCB.Core.Infra.CrossCutting.DesignPatterns.ExecutionInfo.Validators.Interfaces;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Validator;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.ExecutionInfo.Validators;
public class ExecutionUserValidator
    : ValidatorBase<Abstractions.ExecutionInfo.ExecutionInfo>,
    IExecutionUserValidator
{
    // Properties
    protected IExecutionInfoSpecifications ExecutionInfoSpecifications { get; }

    // Constructors
    protected ExecutionUserValidator(IExecutionInfoSpecifications executionInfoSpecifications)
    {
        ExecutionInfoSpecifications = executionInfoSpecifications;
    }

    // Protected Methods
    protected override void ConfigureFluentValidationConcreteValidator(FluentValidationValidatorWrapper fluentValidationValidatorWrapper)
    {
        fluentValidationValidatorWrapper.RuleFor(input => input.TenantId)
            .Must(ExecutionInfoSpecifications.TenantIdShouldRequired)
            .WithErrorCode(IExecutionUserValidator.InputBaseShouldHaveTenantIdErrorCode)
            .WithMessage(IExecutionUserValidator.InputBaseShouldHaveTenantIdMessage)
            .WithSeverity(IExecutionUserValidator.InputBaseShouldHaveTenantIdSeverity);

        fluentValidationValidatorWrapper.RuleFor(input => input.ExecutionUser)
            .Must(ExecutionInfoSpecifications.ExecutionUserShouldRequired)
            .WithErrorCode(IExecutionUserValidator.InputBaseShouldHaveExecutionUserErrorCode)
            .WithMessage(IExecutionUserValidator.InputBaseShouldHaveExecutionUserMessage)
            .WithSeverity(IExecutionUserValidator.InputBaseShouldHaveExecutionUserSeverity);

        fluentValidationValidatorWrapper.RuleFor(input => input.ExecutionUser)
            .Must(ExecutionInfoSpecifications.ExecutionUserShouldValid)
            .When(inputBase => ExecutionInfoSpecifications.ExecutionUserShouldRequired(inputBase.ExecutionUser))
            .WithErrorCode(IExecutionUserValidator.InputBaseShouldHaveExecutionUserWithValidLengthErrorCode)
            .WithMessage(IExecutionUserValidator.InputBaseShouldHaveExecutionUserWithValidLengthMessage)
            .WithSeverity(IExecutionUserValidator.InputBaseShouldHaveExecutionUserWithValidLengthSeverity);

        fluentValidationValidatorWrapper.RuleFor(input => input.SourcePlatform)
            .Must(ExecutionInfoSpecifications.SourcePlatformShouldRequired)
            .WithErrorCode(IExecutionUserValidator.InputBaseShouldHaveSourcePlatformErrorCode)
            .WithMessage(IExecutionUserValidator.InputBaseShouldHaveSourcePlatformMessage)
            .WithSeverity(IExecutionUserValidator.InputBaseShouldHaveSourcePlatformSeverity);

        fluentValidationValidatorWrapper.RuleFor(input => input.SourcePlatform)
            .Must(ExecutionInfoSpecifications.SourcePlatformShouldValid)
            .When(inputBase => ExecutionInfoSpecifications.SourcePlatformShouldRequired(inputBase.SourcePlatform))
            .WithErrorCode(IExecutionUserValidator.InputBaseShouldHaveSourcePlatformWithValidLengthErrorCode)
            .WithMessage(IExecutionUserValidator.InputBaseShouldHaveSourcePlatformWithValidLengthMessage)
            .WithSeverity(IExecutionUserValidator.InputBaseShouldHaveSourcePlatformWithValidLengthSeverity);


        fluentValidationValidatorWrapper.RuleFor(input => input.CorrelationId)
            .Must(ExecutionInfoSpecifications.CorrelationIdShouldRequired)
            .WithErrorCode(IExecutionUserValidator.InputBaseShouldHaveCorrelationIdErrorCode)
            .WithMessage(IExecutionUserValidator.InputBaseShouldHaveCorrelationIdMessage)
            .WithSeverity(IExecutionUserValidator.InputBaseShouldHaveCorrelationIdSeverity);
    }
}
