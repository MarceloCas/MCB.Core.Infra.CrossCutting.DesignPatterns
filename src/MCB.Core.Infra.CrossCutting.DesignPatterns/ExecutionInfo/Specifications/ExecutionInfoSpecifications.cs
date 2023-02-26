using MCB.Core.Infra.CrossCutting.DesignPatterns.ExecutionInfo.Specifications.Interfaces;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.ExecutionInfo.Specifications;
public class ExecutionInfoSpecifications
    : IExecutionInfoSpecifications
{
    public bool TenantIdShouldRequired(Guid tenantId)
    {
        return tenantId != Guid.Empty;
    }

    public bool ExecutionUserShouldRequired(string executionUser)
    {
        return !string.IsNullOrWhiteSpace(executionUser);
    }
    public virtual bool ExecutionUserShouldValid(string executionUser)
    {
        return ExecutionUserShouldRequired(executionUser)
            && executionUser.Length <= IExecutionInfoSpecifications.EXECUTION_INFO_EXECUTION_USER_MAX_LENGTH;
    }

    public bool SourcePlatformShouldRequired(string sourcePlatform)
    {
        return !string.IsNullOrWhiteSpace(sourcePlatform);
    }
    public virtual bool SourcePlatformShouldValid(string sourcePlatform)
    {
        return SourcePlatformShouldRequired(sourcePlatform)
            && sourcePlatform.Length <= IExecutionInfoSpecifications.EXECUTION_INFO_SOURCE_PLATFORM_MAX_LENGTH;
    }

    public bool CorrelationIdShouldRequired(Guid correlationId)
    {
        return correlationId != Guid.Empty;
    }
}