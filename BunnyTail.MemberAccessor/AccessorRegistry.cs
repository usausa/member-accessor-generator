namespace BunnyTail.MemberAccessor;

public static class AccessorRegistry
{
    private static readonly Dictionary<Type, Type> AccessorTypes = [];

    private static readonly Dictionary<Type, IAccessor> Accessors = [];

    private static readonly Dictionary<Type, Type> FactoryTypes = [];

    private static readonly Dictionary<Type, IAccessorFactory> Factories = [];

    public static void RegisterFactory(Type type, Type accessorType, Type factoryType)
    {
        AccessorTypes[type] = accessorType;
        FactoryTypes[type] = factoryType;
    }

    public static IAccessor? FindAccessor<T>() => FindAccessor(typeof(T));

    public static IAccessor? FindAccessor(Type type)
    {
        lock (Accessors)
        {
            if (!Accessors.TryGetValue(type, out var accessor))
            {
                if (type.IsGenericType)
                {
                    if (!AccessorTypes.TryGetValue(type.GetGenericTypeDefinition(), out var openAccessorType))
                    {
                        return null;
                    }

                    var factoryType = openAccessorType.MakeGenericType(type.GenericTypeArguments);
                    accessor = (IAccessor)Activator.CreateInstance(factoryType)!;
                }
                else
                {
                    if (!AccessorTypes.TryGetValue(type, out var factoryType))
                    {
                        return null;
                    }

                    accessor = (IAccessor)Activator.CreateInstance(factoryType)!;
                }

                Accessors[type] = accessor;
            }

            return accessor;
        }
    }

    public static IAccessorFactory<T>? FindFactory<T>() => (IAccessorFactory<T>?)ResolveFactory(typeof(T));

    public static IAccessorFactory? FindFactory(Type type) => ResolveFactory(type);

    private static IAccessorFactory? ResolveFactory(Type type)
    {
        lock (Factories)
        {
            if (!Factories.TryGetValue(type, out var factory))
            {
                if (type.IsGenericType)
                {
                    if (!FactoryTypes.TryGetValue(type.GetGenericTypeDefinition(), out var openFactoryType))
                    {
                        return null;
                    }

                    var factoryType = openFactoryType.MakeGenericType(type.GenericTypeArguments);
                    factory = (IAccessorFactory)Activator.CreateInstance(factoryType)!;
                }
                else
                {
                    if (!FactoryTypes.TryGetValue(type, out var factoryType))
                    {
                        return null;
                    }

                    factory = (IAccessorFactory)Activator.CreateInstance(factoryType)!;
                }

                Factories[type] = factory;
            }

            return factory;
        }
    }
}
