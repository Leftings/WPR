namespace Test.Unit;

using Test.Unit.Mocks;
using WPR.Hashing;

public class HashTest
{
    private readonly EnvConfigMock _env;
    private readonly Hash _hash;

    public HashTest()
    {
        _env = new EnvConfigMock();
        _hash = new Hash(_env);
    }

    private string CreateHash()
    {
        return _hash.createHash("FakePassWord");
    }

    [Fact]
    public void GoodHashWrongMatch()
    {
        Assert.DoesNotMatch("FakePassWord", CreateHash());
    }

    [Fact]
    public void GoodHashGoodMatch()
    {
        string hashed = _hash.createHash("FakePassWord");
        Assert.Equal(hashed, CreateHash());
    }

    [Fact]
    public void GoodHashWrongMatchHashed()
    {
        string hashed = _hash.createHash("WRONG");
        Assert.DoesNotMatch(hashed, CreateHash());
    }
}