namespace MemberAccessorGenerator;

using System.Collections.Concurrent;

public static class AccessorRegistry
{
    private static readonly ConcurrentDictionary<Type, IAccessorFactory> Factories = new();

    public static void RegisterFactory<T>(IAccessorFactory<T> factory)
    {
        Factories[typeof(T)] = factory;
    }

    public static IAccessorFactory? FindFactory(Type type) =>
        Factories.GetValueOrDefault(type);

    public static IAccessorFactory<T>? FindFactory<T>() =>
        Factories.TryGetValue(typeof(T), out var accessor) ? (IAccessorFactory<T>)accessor : null;
}
