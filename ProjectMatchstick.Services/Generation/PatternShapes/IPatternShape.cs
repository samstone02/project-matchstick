using Godot;
using System.Collections.Generic;

namespace ProjectMatchstick.Services.Generation.PatternShapes;

public interface IPatternShape
{
    public IEnumerable<int> SuperimposedRotations { get; }

    public IEnumerable<Vector2I> Cells { get; }

    public Dictionary<Vector2I, T> RotatePattern<T>(Dictionary<Vector2I, T> pattern, int degrees);

    /// <summary>
    /// Are the two vectors orthogonal?
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns>Wether the vectors are orthogonal or not.</returns>
    public bool IsOrthogonal(Vector2I vector1, Vector2I vector2);
}
