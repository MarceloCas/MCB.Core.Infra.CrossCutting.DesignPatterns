using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using Mapster;
using MapsterMapper;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Adapter;

namespace Bechmarks.Benchs.AdapterBenchs;

[SimpleJob(RunStrategy.Throughput, launchCount: 1)]
[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
[MemoryDiagnoser]
[HtmlExporter]
public class AdapterBench
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
    public void AdaptSingleInstance()
    {
        var adapter = CreateAdapter();
        var customerDataModel = new CustomerDataModel
        {
            Id = Guid.NewGuid(),
            FirstName = new string('a', 50),
            LastName = new string('a', 150),
            BirthDate = DateTime.UtcNow.AddYears(-21),
            IsActive = true
        };

        for (int i = 0; i < IterationCount; i++)
            _ = adapter.Adapt<CustomerDataModel, CustomerViewModel>(customerDataModel);
    }
    [Benchmark()]
    public void AdaptMultiInstance()
    {
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
            var adapter = CreateAdapter();
            _ = adapter.Adapt<CustomerDataModel, CustomerViewModel>(customerDataModel);
        }
    }

    [Benchmark()]
    public void AdaptSingleInstanceParallel()
    {
        var adapter = CreateAdapter();
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
            _ = adapter.Adapt<CustomerDataModel, CustomerViewModel>(customerDataModel);
        });
    }
    [Benchmark()]
    public void AdaptMultiInstanceParallel()
    {
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
            var adapter = CreateAdapter();
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