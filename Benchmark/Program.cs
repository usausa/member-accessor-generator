namespace Benchmark;

using System.Linq.Expressions;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Running;

using MemberAccessorGenerator;

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
    }
}

[Config(typeof(BenchmarkConfig))]
public class Benchmark
{
    private const int N = 1000;

    private static readonly Data Data = new();

    private Func<Data, int> expressionGetter = default!;
    private Func<Data, int> generatorGetter = default!;
    private Action<Data, int> expressionSetter = default!;
    private Action<Data, int> generatorSetter = default!;

    [GlobalSetup]
    public void Setup()
    {
        expressionGetter = ExpressionHelper.CreateGetter<Data, int>(nameof(Data.Id));
        expressionSetter = ExpressionHelper.CreateSetter<Data, int>(nameof(Data.Id));

        var accessorFactory = AccessorRegistry.FindFactory<Data>()!;
        generatorGetter = accessorFactory.CreateGetter<int>(nameof(Data.Id))!;
        generatorSetter = accessorFactory.CreateSetter<int>(nameof(Data.Id))!;
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void ExpressionGetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            expressionGetter(o);
        }
    }

    [Benchmark(OperationsPerInvoke = N)]
    public void GeneratorGetter()
    {
        var o = Data;
        for (var i = 0; i < N; i++)
        {
            generatorGetter(o);
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
