﻿using Godot;
using ProjectMatchstick.Services.Generation.Steps;
using ProjectMatchstick.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using static ProjectMatchstick.Services.Generation.Steps.OverlappedWfcGenerationStep;

namespace ProjectMatchstick.Services.Generation.PatternShapes;

public class SqaurePatternShape : IPatternShape
{
    private readonly List<int> _rotations = new List<int> { 0, 90, 180, 270 };

    private readonly List<Vector2I> _cells;

    public IEnumerable<int> SuperimposedRotations => _rotations;

    public bool CanCloseGaps { get => Size != 2; }

    public IEnumerable<Vector2I> Cells => _cells;

    public int Size { get; }

    public SqaurePatternShape(int size)
    {
        _cells = new List<Vector2I>();

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                _cells.Add(new Vector2I(i, j));
            }
        }

        Size = size;
    }

    public Dictionary<Vector2I, T> RotatePattern<T>(Dictionary<Vector2I, T> pattern, int degrees)
    {
        degrees %= 360;

        if (!_rotations.Contains(degrees))
        {
            throw new NotSupportedException($"A {degrees} degree rotation does result in a superposition for {nameof(SqaurePatternShape)}.");
        }

        if (pattern.Count != _cells.Count || pattern.Where(kv => _cells.Contains(kv.Key)).Count() != _cells.Count)
        {
            throw new ArgumentException("The pattern is not of the expected size.");
        }

        var matrix = MapHelper.ToMatrix(pattern);

        var turns = degrees / 90;

        var rotatedMatrix = MatrixHelper.RotateClockwise(matrix, turns);

        return MatrixHelper.ToVectorDictionary(rotatedMatrix);
    }

    public bool AreAdjacent(Vector2I vector1, Vector2I vector2)
    {
        return Math.Abs(vector1.X - vector2.X) == 0 && Math.Abs(vector1.Y - vector2.Y) == 1
            || Math.Abs(vector1.X - vector2.X) == 1 && Math.Abs(vector1.Y - vector2.Y) == 0;
    }

    public List<Vector2I> GetAdjacencies(Vector2I position)
    {
        return new List<Vector2I>
        {
            new Vector2I(position.X + 1, position.Y),
            new Vector2I(position.X - 1, position.Y),
            new Vector2I(position.X, position.Y + 1),
            new Vector2I(position.X, position.Y - 1),
        };
    }
}
