using Godot;

namespace ProjectMatchstick.Generation.Steps;

public interface IGenerationStep
{
    void Generate(TileMap tileMap, Vector2I topCorner, Vector2I bottomCorner);
}
