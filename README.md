# MemberAccessorGenerator

[![NuGet Badge](https://buildstats.info/nuget/MemberAccessorGenerator)](https://www.nuget.org/packages/MemberAccessorGenerator/)

## Reference

Add reference to MemberAccessorGenerator and MemberAccessorGenerator.SourceGenerator to csproj.

```xml
  <ItemGroup>
    <PackageReference Include="MemberAccessorGenerator" Version="0.1.0" />
    <PackageReference Include="MemberAccessorGenerator.SourceGenerator" Version="0.1.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
```

## MemberAccessor

### Source

```csharp
[GenerateAccessor]
public partial class Data
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
}
```

```csharp
var accessorFactory = AccessorRegistry.FindFactory<Data>();
var getter = accessorFactory.CreateGetter<int>(nameof(Data.Id));
var setter = accessorFactory.CreateSetter<int>(nameof(Data.Id));

var data = new Data();
setter(data, 123);
var id = getter(data);
```

## Benchmark

```
BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3235/23H2/2023Update/SunValley3)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.204
  [Host]     : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
```
| Method           | Mean      | Error     | StdDev    | Min       | Max       | P90       | Code Size | Allocated |
|----------------- |----------:|----------:|----------:|----------:|----------:|----------:|----------:|----------:|
| ExpressionGetter | 1.1036 ns | 0.0181 ns | 0.0169 ns | 1.0813 ns | 1.1248 ns | 1.1205 ns |      57 B |         - |
| GeneratorGetter  | 0.2930 ns | 0.0016 ns | 0.0015 ns | 0.2908 ns | 0.2956 ns | 0.2948 ns |      75 B |         - |
| ExpressionSetter | 1.2986 ns | 0.0073 ns | 0.0068 ns | 1.2891 ns | 1.3086 ns | 1.3081 ns |      60 B |         - |
| GeneratorSetter  | 0.4361 ns | 0.0014 ns | 0.0011 ns | 0.4340 ns | 0.4378 ns | 0.4372 ns |      83 B |         - |
