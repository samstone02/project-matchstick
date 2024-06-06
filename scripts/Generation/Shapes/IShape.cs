using Godot;
using System.Collections.Generic;

namespace ProjectMatchstick.Generation.Shapes;

public interface IShape
{
    public bool IsInside(Vector2 coords);
    public List<Vector2I> ToList();
}
