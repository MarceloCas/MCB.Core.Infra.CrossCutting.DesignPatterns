using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using Mapster;
using MapsterMapper;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Adapter;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Adapter;
using Microsoft.Extensions.DependencyInjection;

namespace Bechmarks.Benchs.AdapterBenchs;

[SimpleJob(RunStrategy.Throughput, launchCount: 1)]
[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
[MemoryDiagnoser]
[HtmlExporter]
public class AdaptersWithIoCBench
{
    [Params(1, 10, 50)]
    public int IterationCount { get; set; }

    private static Adapter CreateAdapter()
    {
        var typeAdapterConfig = new TypeAdapterConfig();
        typeAdapterConfig.ForType<CustomerDataModel, CustomerViewModel>();

        return new Adapter(new Mapper());
    }

    [Benchmark(Baseline = true)]
    public void AdaptWithSingletonIoCInstance()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAdapter>(serviceProvider => CreateAdapter());
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var customerDataModel = new CustomerDataModel
        {
            Id = Guid.NewGuid(),
            FirstName = new string('a', 50),
            LastName = new string('a', 150),
            BirthDate = DateTime.UtcNow.AddYears(-21),
            IsActive = true
        };

        for (int i = 0; i < IterationCount; i++)
        {
            var adapter = serviceProvider.GetRequiredService<IAdapter>();
            _ = adapter.Adapt<CustomerDataModel, CustomerViewModel>(customerDataModel);
        }
    }
    [Benchmark()]
    public void AdaptWithTransientIoCInstance()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IAdapter>(serviceProvider => CreateAdapter());
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var customerDataModel = new CustomerDataModel
        {
            Id = Guid.NewGuid(),
            FirstName = new string('a', 50),
            LastName = new string('a', 150),
            BirthDate = DateTime.UtcNow.AddYears(-21),
            IsActive = true
        };

        for (int i = 0; i < IterationCount; i++)
        {
            var adapter = serviceProvider.GetRequiredService<IAdapter>();
            _ = adapter.Adapt<CustomerDataModel, CustomerViewModel>(customerDataModel);
        }
    }
    [Benchmark()]
    public void AdaptWithScopedIoCInstance()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IAdapter>(serviceProvider => CreateAdapter());
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var customerDataModel = new CustomerDataModel
        {
            Id = Guid.NewGuid(),
            FirstName = new string('a', 50),
            LastName = new string('a', 150),
            BirthDate = DateTime.UtcNow.AddYears(-21),
            IsActive = true
        };

        for (int i = 0; i < IterationCount; i++)
        {
            var scope = serviceProvider.CreateScope();
            var adapter = scope.ServiceProvider.GetRequiredService<IAdapter>();
            _ = adapter.Adapt<CustomerDataModel, CustomerViewModel>(customerDataModel);
        }
    }

    [Benchmark()]
    public void AdaptWithSingletonIoCInstanceParallel()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAdapter>(serviceProvider => CreateAdapter());
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var customerDataModel = new CustomerDataModel
        {
            Id = Guid.NewGuid(),
            FirstName = new string('a', 50),
            LastName = new string('a', 150),
            BirthDate = DateTime.UtcNow.AddYears(-21),
            IsActive = true
        };

        Parallel.For(0, IterationCount, i =>
        {
            var adapter = serviceProvider.GetRequiredService<IAdapter>();
            _ = adapter.Adapt<CustomerDataModel, CustomerViewModel>(customerDataModel);
        });
    }
    [Benchmark()]
    public void AdaptWithTransientIoCInstanceParallel()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IAdapter>(serviceProvider => CreateAdapter());
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var customerDataModel = new CustomerDataModel
        {
            Id = Guid.NewGuid(),
            FirstName = new string('a', 50),
            LastName = new string('a', 150),
            BirthDate = DateTime.UtcNow.AddYears(-21),
            IsActive = true
        };

        Parallel.For(0, IterationCount, i =>
        {
            var adapter = serviceProvider.GetRequiredService<IAdapter>();
            _ = adapter.Adapt<CustomerDataModel, CustomerViewModel>(customerDataModel);
        });
    }
    [Benchmark()]
    public void AdaptWithScopedIoCInstanceParallel()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IAdapter>(serviceProvider => CreateAdapter());
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var customerDataModel = new CustomerDataModel
        {
            Id = Guid.NewGuid(),
            FirstName = new string('a', 50),
            LastName = new string('a', 150),
            BirthDate = DateTime.UtcNow.AddYears(-21),
            IsActive = true
        };

        Parallel.For(0, IterationCount, i =>
        {
            var scope = serviceProvider.CreateScope();
            var adapter = scope.ServiceProvider.GetRequiredService<IAdapter>();
            _ = adapter.Adapt<CustomerDataModel, CustomerViewModel>(customerDataModel);
        });
    }

    public class CustomerDataModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; }
    }
    public class CustomerViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; }
    }
}

