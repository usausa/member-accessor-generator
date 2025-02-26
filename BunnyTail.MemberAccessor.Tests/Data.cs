namespace BunnyTail.MemberAccessor;

[GenerateAccessor]
public class Data
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
}

[GenerateAccessor]
public class NullableData
{
    public int? Id { get; set; }

    public string? Name { get; set; }
}

[GenerateAccessor]
[TypedAccessor(typeof(GenericData<DateTime>))]
[TypedAccessor(typeof(GenericData<short>))]
public class GenericData<T>
{
    public T Value { get; set; } = default!;
}

[GenerateAccessor]
[TypedAccessor(typeof(MultiGenericData<string, string>))]
public class MultiGenericData<T1, T2>
{
    public T1 Value1 { get; set; } = default!;

    public T2 Value2 { get; set; } = default!;
}
