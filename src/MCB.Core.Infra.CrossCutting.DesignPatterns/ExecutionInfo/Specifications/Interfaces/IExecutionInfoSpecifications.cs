namespace MCB.Core.Infra.CrossCutting.DesignPatterns.ExecutionInfo.Specifications.Interfaces;
public interface IExecutionInfoSpecifications
{
    const int EXECUTION_INFO_EXECUTION_USER_MAX_LENGTH = 150;
    const int EXECUTION_INFO_SOURCE_PLATFORM_MAX_LENGTH = 150;

    bool TenantIdShouldRequired(Guid tenantId);

    bool ExecutionUserShouldRequired(string executionUser);
    bool ExecutionUserShouldValid(string executionUser);

    bool SourcePlatformShouldRequired(string sourcePlatform);
    bool SourcePlatformShouldValid(string sourcePlatform);

    bool CorrelationIdShouldRequired(Guid correlationId);
}
