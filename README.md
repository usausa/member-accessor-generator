# BunnyTail.MemberAccessor

[![NuGet](https://img.shields.io/nuget/v/BunnyTail.MemberAccessor.svg)](https://www.nuget.org/packages/BunnyTail.MemberAccessor)

## Reference

Add reference to BunnyTail.MemberAccessor to csproj.

```xml
  <ItemGroup>
    <PackageReference Include="BunnyTail.MemberAccessor" Version="1.2.0" />
  </ItemGroup>
```

## MemberAccessor

### Source

```csharp
using BunnyTail.MemberAccessor;

[GenerateAccessor]
public partial class Data
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
}
```

```csharp
using BunnyTail.MemberAccessor;

var accessorFactory = AccessorRegistry.FindFactory<Data>();
var getter = accessorFactory.CreateGetter<int>(nameof(Data.Id));
var setter = accessorFactory.CreateSetter<int>(nameof(Data.Id));

var data = new Data();
setter(data, 123);
var id = getter(data);
```

## Benchmark

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
```
| Method               | Mean       | Error     | StdDev    | Median     | Min        | Max        | P90        | Gen0   | Code Size | Allocated |
|--------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-----------:|-------:|----------:|----------:|
| DirectGetter         |  0.2474 ns | 0.0051 ns | 0.0075 ns |  0.2461 ns |  0.2340 ns |  0.2639 ns |  0.2573 ns |      - |      10 B |         - |
| PropertyGetter       | 28.2982 ns | 0.3361 ns | 0.4926 ns | 28.2985 ns | 27.3895 ns | 29.2224 ns | 28.8837 ns | 0.0014 |   6,737 B |      24 B |
| PropertyGetterCashed | 12.0477 ns | 0.2747 ns | 0.4112 ns | 12.0093 ns | 11.2672 ns | 12.9446 ns | 12.6706 ns | 0.0014 |   2,877 B |      24 B |
| AccessorGetter       | 19.6870 ns | 1.5183 ns | 2.2255 ns | 18.5110 ns | 17.1518 ns | 23.6697 ns | 22.6651 ns | 0.0014 |        NA |      24 B |
| AccessorGetterCached |  2.9748 ns | 0.0438 ns | 0.0614 ns |  2.9845 ns |  2.8550 ns |  3.1234 ns |  3.0399 ns | 0.0014 |     174 B |      24 B |
| ExpressionGetter     |  1.4170 ns | 0.0176 ns | 0.0247 ns |  1.4083 ns |  1.3824 ns |  1.4858 ns |  1.4424 ns |      - |      54 B |         - |
| GeneratorGetter      |  0.2493 ns | 0.0051 ns | 0.0077 ns |  0.2484 ns |  0.2346 ns |  0.2673 ns |  0.2576 ns |      - |      76 B |         - |
| DirectSetter         |  0.2461 ns | 0.0046 ns | 0.0069 ns |  0.2464 ns |  0.2319 ns |  0.2602 ns |  0.2559 ns |      - |      28 B |         - |
| PropertySetter       | 30.9568 ns | 0.7685 ns | 1.1264 ns | 30.9573 ns | 28.9010 ns | 33.1765 ns | 32.5602 ns | 0.0014 |   7,622 B |      24 B |
| PropertySetterCashed | 14.8384 ns | 0.3280 ns | 0.4910 ns | 14.7831 ns | 14.0345 ns | 15.9141 ns | 15.4103 ns | 0.0014 |   3,747 B |      24 B |
| AccessorSetter       | 18.5967 ns | 0.4391 ns | 0.6572 ns | 18.6379 ns | 17.2392 ns | 19.8214 ns | 19.3874 ns | 0.0014 |        NA |      24 B |
| AccessorSetterCached |  2.7250 ns | 0.0634 ns | 0.0949 ns |  2.7154 ns |  2.5550 ns |  2.9903 ns |  2.8612 ns | 0.0014 |     191 B |      24 B |
| ExpressionSetter     |  1.4299 ns | 0.0171 ns | 0.0255 ns |  1.4219 ns |  1.3967 ns |  1.4988 ns |  1.4633 ns |      - |      57 B |         - |
| GeneratorSetter      |  0.4795 ns | 0.0071 ns | 0.0105 ns |  0.4783 ns |  0.4610 ns |  0.5038 ns |  0.4926 ns |      - |      85 B |         - |
