namespace BunnyTail.MemberAccessor;

public static class AccessorRegistry
{
    private static readonly Dictionary<Type, Type> TypeRegistry = [];

    private static readonly Dictionary<Type, IAccessorFactory> Factories = [];

    public static void RegisterFactory(Type type, Type accessorType)
    {
        TypeRegistry[type] = accessorType;
    }

    public static IAccessorFactory? FindFactory(Type type) => ResolveFactory(type);

    public static IAccessorFactory<T>? FindFactory<T>() => (IAccessorFactory<T>?)ResolveFactory(typeof(T));

    private static IAccessorFactory? ResolveFactory(Type type)
    {
        lock (Factories)
        {
            if (!Factories.TryGetValue(type, out var accessor))
            {
                if (type.IsGenericType)
                {
                    if (!TypeRegistry.TryGetValue(type.GetGenericTypeDefinition(), out var openAccessorType))
                    {
                        return null;
                    }

                    var accessorType = openAccessorType.MakeGenericType(type.GenericTypeArguments);
                    accessor = (IAccessorFactory)Activator.CreateInstance(accessorType)!;
                }
                else
                {
                    if (!TypeRegistry.TryGetValue(type, out var accessorType))
                    {
                        return null;
                    }

                    accessor = (IAccessorFactory)Activator.CreateInstance(accessorType)!;
                }

                Factories[type] = accessor;
            }

            return accessor;
        }
    }
}
