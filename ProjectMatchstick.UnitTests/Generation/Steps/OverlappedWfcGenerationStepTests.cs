using Godot;
using ProjectMatchstick.Services.Generation.Steps;
using static ProjectMatchstick.Services.Generation.Steps.OverlappedWfcGenerationStep;

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

    #region ExtractUniquePatterns

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

        Assert.Equal(10, uniquePatterns.Count);
        AssertUniqueItems(uniquePatterns);
    }

    #endregion

    #region CanApply

    [Fact]
    public void CallApplyPattern_With2x2EmptyTargetAndValidPattern_ReturnsTrue()
    {
        var wfc = new OverlappedWfcGenerationStep();
        var map = new Dictionary<Vector2I, Cell>();

        var targetCellsSet = new HashSet<Vector2I>
        {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1),
        };

        var pattern = new Pattern
        {
            Cells = new Dictionary<Vector2I, int>
            {
                { new(0, 0), 1 },
                { new(0, 1), 1 },
                { new(1, 0), 1 },
                { new(1, 1), 1 },
            }
        };

        var canApply = wfc.CanApplyPatternAt(map, pattern, new(0,0));

        Assert.True(canApply);
    }
    
    [Fact]
    public void CallApplyPattern_With2x2TargetAndCollapsedCornerAndValidPattern_ReturnsTrue()
    {
        var wfc = new OverlappedWfcGenerationStep();
        
        var map = new Dictionary<Vector2I, Cell>
        {
            { new(0, 0), new() { Terrain = 1 } }
        };

        var targetCellsSet = new HashSet<Vector2I>
        {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1),
        };

        var pattern = new Pattern
        {
            Cells = new Dictionary<Vector2I, int>
            {
                { new(0, 0), 1 },
                { new(0, 1), 2 },
                { new(1, 0), 2 },
                { new(1, 1), 2 },
            }
        };

        var canApply = wfc.CanApplyPatternAt(map, pattern, new(0, 0));

        Assert.True(canApply);
    }

    [Fact]
    public void CallApplyPattern_With2x2TargetAnd2CollapsedCornersAndValidPattern_ReturnsTrue()
    {
        var wfc = new OverlappedWfcGenerationStep();

        var map = new Dictionary<Vector2I, Cell>
        {
            { new(0, 0), new() { Terrain = 1 } },
            { new(1, 1), new() { Terrain = 1 } }
        };

        var targetCellsSet = new HashSet<Vector2I>
        {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1),
        };

        var pattern = new Pattern
        {
            Cells = new Dictionary<Vector2I, int>
            {
                { new(0, 0), 1 },
                { new(0, 1), 2 },
                { new(1, 0), 2 },
                { new(1, 1), 1 },
            }
        };

        var canApply = wfc.CanApplyPatternAt(map, pattern, new(0, 0));

        Assert.True(canApply);
    }

    [Fact]
    public void CallApplyPattern_With2x2TargetAndCollapsedCornerAndInvalidPattern_ReturnsFalse()
    {
        var wfc = new OverlappedWfcGenerationStep();

        var map = new Dictionary<Vector2I, Cell>
        {
            { new(0, 0), new() { Terrain = 1 } }
        };

        var targetCellsSet = new HashSet<Vector2I>
        {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1),
        };

        var pattern = new Pattern
        {
            Cells = new Dictionary<Vector2I, int>
            {
                { new(0, 0), 2 },
                { new(0, 1), 2 },
                { new(1, 0), 2 },
                { new(1, 1), 1 },
            }
        };

        var canApply = wfc.CanApplyPatternAt(map, pattern, new(0, 0));

        Assert.False(canApply);
    }

    [Fact]
    public void CallApplyPattern_With2x2TargetAnd2CollapsedCornersAndInvalidPattern_ReturnsFalse()
    {
        var wfc = new OverlappedWfcGenerationStep();

        var map = new Dictionary<Vector2I, Cell>
        {
            { new(0, 0), new() { Terrain = 1 } },
            { new(1, 1), new() { Terrain = 1 } },
        };

        var targetCellsSet = new HashSet<Vector2I>
        {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1),
        };

        var pattern = new Pattern
        {
            Cells = new Dictionary<Vector2I, int>
            {
                { new(0, 0), 1 },
                { new(0, 1), 2 },
                { new(1, 0), 2 },
                { new(1, 1), 3 },
            }
        };

        var canApply = wfc.CanApplyPatternAt(map, pattern, new(0, 0));

        Assert.False(canApply);
    }

    [Fact]
    public void CallApplyPattern_With2x2TargetAndPatternOutOfBounds_ReturnsFalse()
    {
        var wfc = new OverlappedWfcGenerationStep();
        var map = new Dictionary<Vector2I, Cell>();

        var targetCellsSet = new HashSet<Vector2I>
        {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1),
        };

        var pattern = new Pattern
        {
            Cells = new Dictionary<Vector2I, int>
            {
                { new(0, 0), 1 },
                { new(0, 1), 1 },
                { new(1, 0), 1 },
                { new(1, 1), 1 },
            }
        };

        var canApply = wfc.CanApplyPatternAt(map, pattern, new(1, 1));

        Assert.False(canApply);
    }

    [Fact]
    public void CallApplyPattern_With2x2TargetAndApplyCoordinatesOutOfBounds_ReturnsFalse()
    {
        var wfc = new OverlappedWfcGenerationStep();
        var map = new Dictionary<Vector2I, Cell>();

        var targetCellsSet = new HashSet<Vector2I>
        {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1),
        };

        var pattern = new Pattern
        {
            Cells = new Dictionary<Vector2I, int>
            {
                { new(0, 0), 1 },
                { new(0, 1), 1 },
                { new(1, 0), 1 },
                { new(1, 1), 1 },
            }
        };

        var canApply = wfc.CanApplyPatternAt(map, pattern, new(99, 99));

        Assert.False(canApply);
    }

    #endregion

    [Fact]
    public void X()
    {
        var wfc = new OverlappedWfcGenerationStep
        {
            PatternSize = 2,
            Sample = new int[,]
            {
                { 0, 0, 0, 1, 0, },
                { 0, 0, 1, 0, 0, },
                { 0, 1, 0, 0, 0, },
                { 1, 0, 0, 0, 0, },
                { 0, 0, 0, 2, 2, },
            }
        };

        var map = new Dictionary<Vector2I, Cell>();

        var targetCellsSet = new HashSet<Vector2I>
        {
            new(0, 0),
            new(1, 0),
            new(2, 0),
            new(0, 1),
            new(1, 1),
            new(2, 1),
            new(0, 2),
            new(1, 2),
            new(2, 2),
        };

        var frontier = new PriorityQueue<Vector2I, int>();
        frontier.Enqueue(new Vector2I(0, 0), 1);

        var uniquePatterns = wfc.ExtractUniquePatterns();

        var leastChaoticCell = new Vector2I(0, 0);

        wfc.SelectPattern(map, uniquePatterns, leastChaoticCell, null);
    }

    #region GetChaos

    [Fact]
    // TODO: Can you even unit test with TileMap?
    // TODO: Maybe TileMap.GetSurroundingCells is slow... replace with custom implementation? Only use TileMap for final result?
    public void GetChaos_WithValidArgs_ReturnsExpectedChaos()
    {
        var wfc = new OverlappedWfcGenerationStep
        {
            PatternSize = 2,
            Sample = new int[,]
            {
                { 0, 0, 0, 1, 0, },
                { 0, 0, 1, 0, 0, },
                { 0, 1, 0, 0, 0, },
                { 1, 0, 0, 0, 0, },
                { 0, 0, 0, 2, 2, },
            }
        };

        var map = new Dictionary<Vector2I, Cell>();
        
        var tileSet = new TileSet();
            tileSet.AddTerrainSet(0);
                tileSet.AddTerrain(0, 0);
                tileSet.AddTerrain(0, 1);
                tileSet.AddTerrain(0, 2);
        
        var tileMap = new TileMap();
            tileMap.TileSet = tileSet;
            tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { new (0, 0) }, 0, 0);
        
        var targetCellsSet = new HashSet<Vector2I>
        {
            new(0, 0),
            new(1, 0),
            new(2, 0),
            new(0, 1),
            new(1, 1),
            new(2, 1),
            new(0, 2),
            new(1, 2),
            new(2, 2),
        };

        var uniquePatterns = wfc.ExtractUniquePatterns();

        wfc.GetChaosValue(map, new TileMap(), new Vector2I(0, 0), uniquePatterns); 
    }

    #endregion
}
