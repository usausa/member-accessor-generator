namespace MemberAccessorGenerator;

public class MemberAccessorTest
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
    public void TestGenerics()
    {
        var accessorFactory1 = AccessorRegistry.FindFactory<GenericData<int>>();
        var accessorFactory2 = AccessorRegistry.FindFactory<GenericData<string>>();

        Assert.NotNull(accessorFactory1);
        Assert.NotNull(accessorFactory2);

        var get1 = accessorFactory1.CreateGetter<int>(nameof(GenericData<int>.Value));
        var set1 = accessorFactory1.CreateSetter<int>(nameof(GenericData<int>.Value));
        var get2 = accessorFactory2.CreateGetter<string>(nameof(GenericData<string>.Value));
        var set2 = accessorFactory2.CreateSetter<string>(nameof(GenericData<string>.Value));

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
}
