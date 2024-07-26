using Godot;
using ProjectMatchstick.Services.Generation.PatternShapes;

namespace ProjectMatchstick.UnitTests.Generation.PaternShapes;

public class SquarePatternShapesTests
{
    [Theory]
    [InlineData(10, 10, 10, 10, false)]
    [InlineData(10, 10, 11, 10, true)]
    [InlineData(10, 10, 9, 10, true)]
    [InlineData(10, 10, 10, 11, true)]
    [InlineData(10, 10, 10, 9, true)]
    [InlineData(10, 10, 9, 9, false)]
    [InlineData(10, 10, 9, 11, false)]
    [InlineData(10, 10, 11, 9, false)]
    [InlineData(10, 10, 11, 11, false)]
    public void CallIsAdjacent_WithValues_ShouldTestIfAreAdjacent(int x1, int y1, int x2, int y2, bool expected) 
    {
        var shape = new SqaurePatternShape(3);
        var vec1 = new Vector2I(x1, y1);
        var vec2 = new Vector2I(x2, y2);

        bool result = shape.AreAdjacent(vec1, vec2);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CallGetAdjacencies_WithValidPosition_ReturnsCorrectSquareAdjacencies()
    {
        var shape = new SqaurePatternShape(3);
        var vec = new Vector2I(10, 10);
        var expected = new List<Vector2I>
        {
            new Vector2I(9, 10),
            new Vector2I(11, 10),
            new Vector2I(10, 9),
            new Vector2I(10, 11),
        };

        List<Vector2I> result = shape.GetAdjacencies(vec);

        AssertionHelper.AssertContainEqual(expected, result);
    }
}
