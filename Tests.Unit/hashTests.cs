namespace Test.Unit;

using Test.Unit.Mocks;
using WPR.Hashing;
using ZstdSharp.Unsafe;

public class HashTest
{    private string CreateHashedPassword(Hash hash)
    {
        return hash.createHash("FakePassWord");
    }

    private string Hash(Hash hash, string password)
    {
        return hash.createHash(password);
    }

    private Hash CreateHash()
    {
        EnvConfigMock env = new EnvConfigMock();
        Hash hash = new Hash(env);
        return hash;
    }

    [Fact]
    public void GoodHashWrongMatch()
    {
        Hash hash = CreateHash();
        Assert.DoesNotMatch(Hash(hash, "WRONG"), CreateHashedPassword(hash));
    }

    [Fact]
    public void GoodHashGoodMatch()
    {
        Hash hash = CreateHash();
        Assert.Equal(Hash(hash, "FakePassWord"), CreateHashedPassword(hash));
    }
}