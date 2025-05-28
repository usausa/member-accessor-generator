namespace BunnyTail.MemberAccessor;

public class FactoryTest
{
    [Fact]
    public void TestBasic()
    {
        var accessorFactory = AccessorRegistry.FindFactory<Data>();

        Assert.NotNull(accessorFactory);

        var getId = accessorFactory.CreateGetter<int>(nameof(Data.Id));
        var getName = accessorFactory.CreateGetter<string>(nameof(Data.Name));
        var setId = accessorFactory.CreateSetter<int>(nameof(Data.Id));
        var setName = accessorFactory.CreateSetter<string>(nameof(Data.Name));

        Assert.NotNull(getId);
        Assert.NotNull(getName);
        Assert.NotNull(setId);
        Assert.NotNull(setName);

        var data = new Data { Id = 123, Name = "abc" };

        Assert.Equal(123, getId(data));
        Assert.Equal("abc", getName(data));

        setId(data, 234);
        setName(data, "xyz");

        Assert.Equal(234, data.Id);
        Assert.Equal("xyz", data.Name);
    }

    [Fact]
    public void TestNullable()
    {
        var accessorFactory = AccessorRegistry.FindFactory<NullableData>();

        Assert.NotNull(accessorFactory);

        var getId = accessorFactory.CreateGetter<int?>(nameof(NullableData.Id));
        var getName = accessorFactory.CreateGetter<string?>(nameof(NullableData.Name));
        var setId = accessorFactory.CreateSetter<int?>(nameof(NullableData.Id));
        var setName = accessorFactory.CreateSetter<string?>(nameof(NullableData.Name));

        Assert.NotNull(getId);
        Assert.NotNull(getName);
        Assert.NotNull(setId);
        Assert.NotNull(setName);

        var data = new NullableData { Id = 123, Name = "abc" };

        Assert.Equal(123, getId(data));
        Assert.Equal("abc", getName(data));

        setId(data, 234);
        setName(data, "xyz");

        Assert.Equal(234, data.Id);
        Assert.Equal("xyz", data.Name);

        setId(data, null);
        setName(data, null);

        Assert.Null(data.Id);
        Assert.Null(data.Name);
    }

    [Fact]
    public void TestGenerics()
    {
        var accessorFactory1 = AccessorRegistry.FindFactory<GenericData<int>>();
        var accessorFactory2 = AccessorRegistry.FindFactory<GenericData<string>>();

        Assert.NotNull(accessorFactory1);
        Assert.NotNull(accessorFactory2);

        var get1 = accessorFactory1.CreateGetter<int>(nameof(GenericData<>.Value));
        var set1 = accessorFactory1.CreateSetter<int>(nameof(GenericData<>.Value));
        var get2 = accessorFactory2.CreateGetter<string>(nameof(GenericData<>.Value));
        var set2 = accessorFactory2.CreateSetter<string>(nameof(GenericData<>.Value));

        Assert.NotNull(get1);
        Assert.NotNull(set1);
        Assert.NotNull(get2);
        Assert.NotNull(set2);

        var data1 = new GenericData<int> { Value = 123 };

        Assert.Equal(123, get1(data1));

        set1(data1, 234);

        Assert.Equal(234, data1.Value);

        var data2 = new GenericData<string> { Value = "abc" };

        Assert.Equal("abc", get2(data2));

        set2(data2, "xyz");

        Assert.Equal("xyz", data2.Value);
    }

    [Fact]
    public void TestMultiGenerics()
    {
        var accessorFactory1 = AccessorRegistry.FindFactory<MultiGenericData<int, int>>();
        var accessorFactory2 = AccessorRegistry.FindFactory<MultiGenericData<string, string>>();

        Assert.NotNull(accessorFactory1);
        Assert.NotNull(accessorFactory2);

        var get1 = accessorFactory1.CreateGetter<int>(nameof(MultiGenericData<,>.Value1));
        var set1 = accessorFactory1.CreateSetter<int>(nameof(MultiGenericData<,>.Value1));
        var get2 = accessorFactory2.CreateGetter<string>(nameof(MultiGenericData<,>.Value1));
        var set2 = accessorFactory2.CreateSetter<string>(nameof(MultiGenericData<,>.Value1));

        Assert.NotNull(get1);
        Assert.NotNull(set1);
        Assert.NotNull(get2);
        Assert.NotNull(set2);

        var data1 = new MultiGenericData<int, int> { Value1 = 123 };

        Assert.Equal(123, get1(data1));

        set1(data1, 234);

        Assert.Equal(234, data1.Value1);

        var data2 = new MultiGenericData<string, string> { Value1 = "abc" };

        Assert.Equal("abc", get2(data2));

        set2(data2, "xyz");

        Assert.Equal("xyz", data2.Value1);
    }
}
