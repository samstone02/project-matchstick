using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMatchstick.Generation;

public class GenerationUtils
{
    public static bool IsCellOutside(Vector2I cell, Vector2I topCorner, Vector2I bottomCorner)
    {
        return cell.Y < topCorner.Y || cell.Y > bottomCorner.Y || cell.X < topCorner.X || cell.X > bottomCorner.X;
    }
}
