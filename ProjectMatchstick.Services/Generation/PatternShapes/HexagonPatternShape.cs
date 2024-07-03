using Godot;
using ProjectMatchstick.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMatchstick.Services.Generation.PatternShapes;

public class HexagonPatternShape : IPatternShape
{
    private readonly List<int> _rotations = new List<int> { 180 };

    private readonly List<Vector2I> _cells;

    public IEnumerable<int> SuperimposedRotations => _rotations;

    public IEnumerable<Vector2I> Cells => _cells;

    public HexagonPatternShape(int size)
    {
        if (size == 2)
        {
            _cells = new List<Vector2I>
            {
                new(0, 1),
                new(0, 2),
                new(1, 0),
                new(1, 1),
                new(1, 2),
                new(2, 0),
                new(2, 1),
            };
        }
        //else if (size == 3)
        //{
        //    _cells = new List<Vector2I>
        //    {
        //        new(0, 1),
        //        new(0, 2),
        //        new(1, 0),
        //        new(1, 1),
        //        new(1, 2),
        //        new(2, 0),
        //        new(2, 1),
        //    };
        //}
        else
        {
            throw new NotSupportedException($"Hexagon size {size} is not yet supported.");
        }
    }

    public Dictionary<Vector2I, T> RotatePattern<T>(Dictionary<Vector2I, T> pattern, int degrees)
    {
        degrees %= 360;

        if (!_rotations.Contains(degrees))
        {
            throw new NotSupportedException($"A {degrees} degree rotation does result in a superposition for {nameof(HexagonPatternShape)}.");
        }

        if (pattern.Count != _cells.Count || pattern.Where(kv => _cells.Contains(kv.Key)).Count() != _cells.Count)
        {
            throw new ArgumentException("The pattern is not of the expected size.");
        }

        var rotated = MatrixHelper.ToVectorDictionary(
             MatrixHelper.RotateClockwise(
                MapHelper.ToMatrix(pattern), 2));

        return rotated
            .Where(kv => pattern.ContainsKey(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
