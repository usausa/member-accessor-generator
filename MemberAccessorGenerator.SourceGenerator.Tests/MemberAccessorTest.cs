namespace MemberAccessorGenerator;

public class MemberAccessorTest
{
    [Fact]
    public void TestMemberAccessor()
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
}
