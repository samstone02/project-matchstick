using ProjectMatchstick.Services.Generation.Steps;

namespace ProjectMatchstick.UnitTests.Generation.Steps;

public class OverlappedWfcGenerationStepTests
{
    private void AssertUniqueItems(List<OverlappedWfcGenerationStep.Pattern> patterns) 
    {
        foreach (var pat1 in patterns)
        {
            foreach (var pat2 in patterns)
            {
                if (pat1 == pat2)
                {
                    continue;
                }

                Assert.NotEqual(pat1, pat2);
            }
        }
    }

    [Fact]
    public void CallExtractUniquePatterns_Given2x2SampleAnd2x2PatternSizeAnd4WaySymmety_Returns1UniquePattern()
    {
        var wfc = new OverlappedWfcGenerationStep
        {
            PatternSize = 2,
            Sample = new int[,]
            {
                { 0, 0, },
                { 0, 0, },
            }
        };

        var actualPatterns = wfc.ExtractUniquePatterns();

        AssertUniqueItems(actualPatterns);
        Assert.Single(actualPatterns);
    }

    [Fact]
    public void CallExtractUniquePatterns_Given3x3SampleAnd2x2PatternSize_Returns16UniquePatterns()
    {
        var wfc = new OverlappedWfcGenerationStep
        {
            PatternSize = 2,
            Sample = new int[,]
            {
                { 0, 0, 0, },
                { 0, 1, 1, },
                { 9, 2, 1, },
            }
        };

        var uniquePatterns = wfc.ExtractUniquePatterns();

        Assert.Equal(16, uniquePatterns.Count);
        AssertUniqueItems(uniquePatterns);
    }

    [Fact]
    public void CallExtractUniquePatterns_Given4x4SampleAnd2x2PatternSize_Returns25UniquePatterns()
    {
        var wfc = new OverlappedWfcGenerationStep
        {
            PatternSize = 2,
            Sample = new int[,]
            {
                { 0, 0, 0, 1, },
                { 0, 0, 0, 1, },
                { 0, 1, 1, 4, },
                { 9, 2, 1, 4, },
            }
        };

        var uniquePatterns = wfc.ExtractUniquePatterns();

        Assert.Equal(25, uniquePatterns.Count);
        AssertUniqueItems(uniquePatterns);
    }

    [Fact]
    public void CallExtractUniquePatterns_Given4x4SampleAnd3x3PatternSize_Returns23UniquePatterns()
    {
        var wfc = new OverlappedWfcGenerationStep
        {
            PatternSize = 2,
            Sample = new int[,]
            {
                { 1, 0, 0, 1, },
                { 1, 0, 0, 1, },
                { 4, 1, 2, 4, },
                { 4, 2, 1, 4, },
            }
        };

        var uniquePatterns = wfc.ExtractUniquePatterns();

        Assert.Equal(23, uniquePatterns.Count);
        AssertUniqueItems(uniquePatterns);
    }

    [Fact]
    public void CallExtractUniquePatterns_Given4x4SampleAnd3x3PatternSize_Returns16UniquePatterns()
    {
        var wfc = new OverlappedWfcGenerationStep
        {
            PatternSize = 3,
            Sample = new int[,]
            {
                { 0, 0, 0, 1, },
                { 0, 0, 0, 1, },
                { 0, 1, 1, 4, },
                { 9, 2, 1, 4, },
            }
        };

        var uniquePatterns = wfc.ExtractUniquePatterns();

        Assert.Equal(16, uniquePatterns.Count);
        AssertUniqueItems(uniquePatterns);
    }

    [Fact]
    public void CallExtractUniquePatterns_Given5x5SampleAnd4x4PatternSize_Returns7UniquePatterns()
    {
        var wfc = new OverlappedWfcGenerationStep
        {
            PatternSize = 4,
            Sample = new int[,]
            {
                { 0, 0, 0, 1, 0, },
                { 0, 0, 1, 0, 0, },
                { 0, 1, 0, 0, 0, },
                { 1, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, },
            }
        };

        var uniquePatterns = wfc.ExtractUniquePatterns();

        Assert.Equal(7, uniquePatterns.Count);
        AssertUniqueItems(uniquePatterns);
    }
}
