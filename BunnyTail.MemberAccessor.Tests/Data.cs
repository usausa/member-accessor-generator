namespace BunnyTail.MemberAccessor;

[GenerateAccessor]
public class Data
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
}

[GenerateAccessor]
public class GenericData<T>
{
    public T Value { get; set; } = default!;
}
