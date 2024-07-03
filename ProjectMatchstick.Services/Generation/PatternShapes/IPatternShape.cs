using Godot;
using System.Collections.Generic;

namespace ProjectMatchstick.Services.Generation.PatternShapes;

public interface IPatternShape
{
    public IEnumerable<int> SuperimposedRotations { get; }

    public IEnumerable<Vector2I> Cells { get; }

    public Dictionary<Vector2I, T> RotatePattern<T>(Dictionary<Vector2I, T> pattern, int degrees);
}
