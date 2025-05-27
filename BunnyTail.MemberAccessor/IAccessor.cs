namespace BunnyTail.MemberAccessor;

public interface IAccessor
{
    object? GetValue(object obj, string name);

    void SetValue(object obj, string name, object? value);
}
