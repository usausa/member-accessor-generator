# MemberAccessorGenerator

[![NuGet](https://img.shields.io/nuget/v/MemberAccessorGenerator.svg)](https://www.nuget.org/packages/MemberAccessorGenerator)

## Reference

Add reference to MemberAccessorGenerator and MemberAccessorGenerator.SourceGenerator to csproj.

```xml
  <ItemGroup>
    <PackageReference Include="MemberAccessorGenerator" Version="0.3.0" />
    <PackageReference Include="MemberAccessorGenerator.SourceGenerator" Version="0.3.0">
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
| DirectGetter     | 0.2233 ns | 0.0044 ns | 0.0045 ns | 0.2157 ns | 0.2294 ns | 0.2282 ns |      12 B |         - |
| ExpressionGetter | 1.1028 ns | 0.0205 ns | 0.0201 ns | 1.0798 ns | 1.1326 ns | 1.1289 ns |      57 B |         - |
| GeneratorGetter  | 0.2975 ns | 0.0035 ns | 0.0033 ns | 0.2939 ns | 0.3038 ns | 0.3016 ns |      75 B |         - |
| DirectSetter     | 0.2206 ns | 0.0023 ns | 0.0022 ns | 0.2176 ns | 0.2242 ns | 0.2236 ns |      31 B |         - |
| ExpressionSetter | 1.3123 ns | 0.0217 ns | 0.0203 ns | 1.2948 ns | 1.3487 ns | 1.3466 ns |      60 B |         - |
| GeneratorSetter  | 0.4418 ns | 0.0063 ns | 0.0059 ns | 0.4328 ns | 0.4494 ns | 0.4484 ns |      83 B |         - |
