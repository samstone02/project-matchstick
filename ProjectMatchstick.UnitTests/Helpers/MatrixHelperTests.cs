using ProjectMatchstick.Services.Constants;
using ProjectMatchstick.Services.Helpers;

namespace ProjectMatchstick.UnitTests.Helpers;

public class MatrixHelperTests
{
    private static readonly int[,] sample = new int[,]
    {
        { 1, 2, 3, 4, 5},
        { 6, 7, 8, 9,10},
        {11,12,13,14,15},
        {16,17,18,19,20},
        {21,22,23,24,25},
    };

    [Fact]
    public static void CallClone_WithValidMatrix_CloneShouldBeEqual()
    {
        var clone = MatrixHelper.Clone(sample);

        Assert.NotSame(sample, clone);
        Assert.Equal(sample, clone);
    }

    [Fact]
    public static void CallGetSubtmatrix_WithEmptyMatrix_ShouldReturnEmptySubmatrix()
    {
        var result = MatrixHelper.GetSubmatrix(new int[,] { }, 0, 0, 0);

        Assert.Empty(result);
    }

    [Fact]
    public static void CallGetSubmatrix_WithCoordsOutOfBounds_ShouldThrowExcepion()
    {
        Assert.Throws<IndexOutOfRangeException>(() => MatrixHelper.GetSubmatrix(sample, 100, 100, 3));
    }

    [Fact]
    public static void CallGetSubmatrix_WithCorodsTooCloseToEdge_ShouldThrowException()
    {
        Assert.Throws<IndexOutOfRangeException>(() => MatrixHelper.GetSubmatrix(sample, 3, 3, 100));

    }

    [Fact]
    public static void CallGetSubmatrix_Given5x5SampleAnd1x1Submatrix_ShouldReturnCorrectSubmatrix()
    {
        var expected = new int[,]
        {
            {13}
        };

        var result = MatrixHelper.GetSubmatrix(sample, 2, 2, 1);

        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallGetSubmatrix_Given5x5SampleAnd2x2Submatrix_ShouldReturnCorrectSubmatrix()
    {
        var expected = new int[,]
        {
           {13,14},
           {18,19},
        };

        var result = MatrixHelper.GetSubmatrix(sample, 2, 2, 2);

        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallGetSubmatrix_Given5x5SampleAnd3x3Submatrix_ShouldReturnCorrectSubmatrix()
    {
        var expected = new int[,]
        {
            {13,14,15},
            {18,19,20},
            {23,24,25},
        };

        var result = MatrixHelper.GetSubmatrix(sample, 2, 2, 3);

        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given1x1InputAndNoRotation_ShouldReturnRotatedMatrix()
    {
        var input = new int[,]
        {
            {13}
        };

        var expected = new int[,]
        {
            {13}
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.NONE);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClochwise_Given2x2InputAndNoRotation_ShouldReturnCorrectRotatedMatrix()
    {
        var input = new int[,]
        {
           {13,14},
           {18,19},
        };

        var expected = new int[,]
        {
           {13,14},
           {18,19},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.NONE);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClochwise_Given3x3InputAndNoRotation_ShouldReturnCorrectRotatedMatrix()
    {
        var input = new int[,]
        {
            {13,14,15},
            {18,19,20},
            {23,24,25},
        };

        var expected = new int[,]
        {
            {13,14,15},
            {18,19,20},
            {23,24,25},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.NONE);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given1x1InputAndQuarterRotation_ShouldReturnCorrectRotatedSubarray()
    {
        var input = new int[,]
        {
            {13}
        };

        var expected = new int[,]
        {
            {13}
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.QUARTER);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given2x2InputAndQuarterRotation_ShouldReturnCorrectRotatedSubarray()
    {
        var input = new int[,]
        {
            {13,14},
            {18,19},
        };

        var expected = new int[,]
        {
           {18,13},
           {19,14},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.QUARTER);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given3x3InputAndQuarterRotation_ShouldReturnCorrectRotatedSubarray()
    {
        var input = new int[,]
        {
            {13,14,15},
            {18,19,20},
            {23,24,25},
        };

        var expected = new int[,]
        {
            {23,18,13},
            {24,19,14},
            {25,20,15},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.QUARTER);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given1x1InputAndHalfRotation_ShouldReturnCorrectRotatedSubarray()
    {
        var input = new int[,]
        {
             {13}
        };

        var expected = new int[,]
        {
            {13}
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.HALF);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given2x2InputAndHalfRotation_ShouldReturnCorrectRotatedSubarray()
    {
        var input = new int[,]
        {
            {13,14},
            {18,19},
        };

        var expected = new int[,]
        {
           {19,18},
           {14,13},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.HALF);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given3x3InputAndHalfRotation_ShouldReturnCorrectRotatedSubarray()
    {
        var input = new int[,]
        {
            {13,14,15},
            {18,19,20},
            {23,24,25},
        };

        var expected = new int[,]
        {
            {25,24,23},
            {20,19,18},
            {15,14,13},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.HALF);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given1x1InputAndThreeQuartersRotation_ShouldReturnCorrectRotatedSubarray()
    {
        var input = new int[,]
        {
             {13}
        };

        var expected = new int[,]
        {
            {13}
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.THREE_QUARTERS);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given2x2InputAndThreeQuartersRotation_ShouldReturnCorrectRotatedSubarray()
    {
        var input = new int[,]
        {
            {13,14},
            {18,19},
        };

        var expected = new int[,]
        {
            {14,19},
            {13,18},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.THREE_QUARTERS);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given3x3InputAndThreeQuartersRotation_ShouldReturnCorrectRotatedSubarray()
    {
        var input = new int[,]
        {
            {13,14,15},
            {18,19,20},
            {23,24,25},
        };

        var expected = new int[,]
        {
            {15,20,25},
            {14,19,24},
            {13,18,23},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.THREE_QUARTERS);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClockwise_Given1x1InputAndFullRotation_ShouldReturnRotatedMatrix()
    {
        var input = new int[,]
        {
            {13}
        };

        var expected = new int[,]
        {
            {13}
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.FULL);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClochwise_Given2x2InputAndFullRotation_ShouldReturnCorrectRotatedMatrix()
    {
        var input = new int[,]
        {
           {13,14},
           {18,19},
        };

        var expected = new int[,]
        {
           {13,14},
           {18,19},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.FULL);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CallRotateClochwise_Given3x3InputAndFullRotation_ShouldReturnCorrectRotatedMatrix()
    {
        var input = new int[,]
        {
            {13,14,15},
            {18,19,20},
            {23,24,25},
        };

        var expected = new int[,]
        {
            {13,14,15},
            {18,19,20},
            {23,24,25},
        };

        var result = MatrixHelper.RotateClockwise(input, Turns.FULL);

        Assert.Same(input, result);
        Assert.Equal(expected, result);
    }
}