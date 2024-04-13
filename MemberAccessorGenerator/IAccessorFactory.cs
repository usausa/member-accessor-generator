namespace MemberAccessorGenerator;

public interface IAccessorFactory
{
    Func<object, object?>? CreateGetter(string name);

    Action<object, object?>? CreateSetter(string name);
}

public interface IAccessorFactory<in T> : IAccessorFactory
{
    Func<T, TProperty>? CreateGetter<TProperty>(string name);

    Action<T, TProperty>? CreateSetter<TProperty>(string name);
}
