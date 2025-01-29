namespace BunnyTail.MemberAccessor;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public sealed class TypedAccessorAttribute : Attribute
{
    public Type ClosedType { get; }

    public TypedAccessorAttribute(Type closedType)
    {
        ClosedType = closedType;
    }
}
