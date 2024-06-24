using ProjectMatchstick.Services.Helpers;

namespace ProjectMatchstick.UnitTests.Helpers;

public class RandomHelperTests
{
    [Fact]
    public void CallSelectRandomWeight_WithEmptyList_ThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RandomHelper.SelectRandomWeighted(new List<int>(), i => i, new Random()));
    }

    [Fact]
    public void CallSelectRandomWeight_WithNullList_ThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RandomHelper.SelectRandomWeighted<int>(null, i => i, new Random()));
    }

    [Fact]
    public void CallSelectRandomWeight_WithENullFuncThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RandomHelper.SelectRandomWeighted(new List<int> { 1, 2, 3}, null, new Random()));
    }

    [Fact]
    public void CallSelectRandomWeight_WithNullRandom_ThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RandomHelper.SelectRandomWeighted(new List<int> { 1, 2, 3}, i => i, null));
    }

    // IDK how to unit test a randomized function... just testing to make sure it doesn't throw exceptions.
    [Fact]
    public void CallSelectRandomWeight_WithIntegerList_ReturnsWeightedRandomValue()
    {
        RandomHelper.SelectRandomWeighted(new List<int> { 1, 2, 3 }, i => i, new Random());
    }
}
