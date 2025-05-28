namespace Benchmark;

using System.Linq.Expressions;
using System.Reflection;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using BunnyTail.MemberAccessor;

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<Benchmark>();
    }
}

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddExporter(MarkdownExporter.GitHub);
        AddColumn(
            StatisticColumn.Mean,
            StatisticColumn.Min,
            StatisticColumn.Max,
            StatisticColumn.P90,
            StatisticColumn.Error,
            StatisticColumn.StdDev);
        AddDiagnoser(MemoryDiagnoser.Default, new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 3, printSource: true, printInstructionAddresses: true, exportDiff: true)));
        AddJob(Job.MediumRun);
    }
}

#pragma warning disable CA1822
[Config(typeof(BenchmarkConfig))]
public class Benchmark
{
    private const int N = 1000;

    private static readonly Data Data = new();

    private PropertyInfo property = default!;

    private IAccessor accessor = default!;

    private Func<Data, int> expressionGetter = default!;
    private Func<Data, int> generatorGetter = default!;
    private Action<Data, int> expressionSetter = default!;
    private Action<Data, int> generatorSetter = default!;

    [GlobalSetup]
    public void Setup()
    {
        property = typeof(Data).GetProperty(nameof(Data.Id))!;

        accessor = AccessorRegistry.FindAccessor<Data>()!;

        expressionGetter = ExpressionHelper.CreateGetter<Data, int>(nameof(Data.Id));
        expressionSetter = ExpressionHelper.CreateSetter<Data, int>(nameof(Data.Id));

        var accessorFactory = AccessorRegistry.FindFactory<Data>()!;
        generatorGetter = accessorFactory.CreateGetter<int>(nameof(Data.Id))!;
        generatorSetter = accessorFactory.CreateSetter<int>(nameof(Data.Id))!;
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void DirectGetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            _ = o.Id;
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void PropertyGetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            var pi = typeof(Data).GetProperty(nameof(Data.Id))!;
            _ = pi.GetValue(o);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void PropertyGetterCashed()
    {
        var o = Data;
        var pi = property;
        for (var i = 0; i < N; i++)
        {
            _ = pi.GetValue(o);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void AccessorGetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            var access = AccessorRegistry.FindAccessor<Data>()!;
            _ = access.GetValue(o, nameof(Data.Id));
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void AccessorGetterCached()
    {
        var o = Data;
        var access = accessor;
        for (var i = 0; i < N; i++)
        {
            _ = access.GetValue(o, nameof(Data.Id));
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void ExpressionGetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            _ = expressionGetter(o);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void GeneratorGetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            _ = generatorGetter(o);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void DirectSetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            o.Id = 0;
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void PropertySetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            var pi = typeof(Data).GetProperty(nameof(Data.Id))!;
            pi.SetValue(o, 0);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void PropertySetterCashed()
    {
        var o = Data;
        var pi = property;
        for (var i = 0; i < N; i++)
        {
            pi.SetValue(o, 0);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void AccessorSetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            var access = AccessorRegistry.FindAccessor<Data>()!;
            access.SetValue(o, nameof(Data.Id), 0);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void AccessorSetterCached()
    {
        var o = Data;
        var access = accessor;
        for (var i = 0; i < N; i++)
        {
            access.SetValue(o, nameof(Data.Id), 0);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void ExpressionSetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            expressionSetter(o, 0);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void GeneratorSetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            generatorSetter(o, 0);
        }
    }
}

[GenerateAccessor]
public class Data
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
}

public static class ExpressionHelper
{
    public static Func<T, TProperty> CreateGetter<T, TProperty>(string name)
    {
        var type = typeof(T);
        var pi = type.GetProperty(name)!;

        var target = Expression.Parameter(type, "target");
        var property = Expression.Property(target, pi);
        var lambda = Expression.Lambda<Func<T, TProperty>>(property, target);
        return lambda.Compile();
    }

    public static Action<T, TProperty> CreateSetter<T, TProperty>(string name)
    {
        var type = typeof(T);
        var pi = type.GetProperty(name)!;

        var target = Expression.Parameter(type, "target");
        var property = Expression.Property(target, pi);
        var value = Expression.Parameter(typeof(TProperty), "value");
        var assign = Expression.Assign(property, value);
        var lambda = Expression.Lambda<Action<T, TProperty>>(assign, target, value);
        return lambda.Compile();
    }
}
