using Godot;
using ProjectMatchstick.Services.Generation.Steps;
using System.Collections.Generic;

namespace ProjectMatchstick.Services.Generation.PatternShapes;

public interface IPatternShape
{
    public IEnumerable<int> SuperimposedRotations { get; }

    /// <summary>
    /// Wether this PatternShape can close gaps without placing adjacently to existing collapsed cells.
    /// </summary>
    public bool CanCloseGaps { get; }

    public IEnumerable<Vector2I> Cells { get; }

    public Dictionary<Vector2I, T> RotatePattern<T>(Dictionary<Vector2I, T> pattern, int degrees);

    /// <summary>
    /// Are the two vectors orthogonal?
    /// </summary>
    public bool AreAdjacent(Vector2I vector1, Vector2I vector2);

    public List<Vector2I> GetAdjacencies(Vector2I position);
}
