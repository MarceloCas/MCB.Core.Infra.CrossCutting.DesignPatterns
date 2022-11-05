using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Tests.Fixtures;

[CollectionDefinition(nameof(DefaultFixture))]
public class DefaultFixtureCollection
    : ICollectionFixture<DefaultFixture>
{

}
public class DefaultFixture
{
    // Properties
    public IServiceProvider ServiceProvider { get; }

    // Constructors
    public DefaultFixture()
    {
        ServiceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();
    }

    // Private Methods
    private static IServiceCollection ConfigureServices(
        IServiceCollection services
    )
    {
        services.AddLogging();

        return services;
    }
}
