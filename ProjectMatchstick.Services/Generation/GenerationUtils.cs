using Godot;

namespace ProjectMatchstick.Services.Generation;

public class GenerationUtils
{
    public static bool IsCellOutside(Vector2I cell, Vector2I topCorner, Vector2I bottomCorner)
    {
        return cell.Y < topCorner.Y || cell.Y > bottomCorner.Y || cell.X < topCorner.X || cell.X > bottomCorner.X;
    }
}
