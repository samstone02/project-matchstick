using Godot;
using ProjectMatchstick.Generation.Steps;

namespace ProjectMatchstick.Generation.Strategies;

/// <summary>
/// Mostly for preprocessing the TileMap. Prevents exceptions on `TileData.Terrain` by setting empty cells to a default terrain.
/// </summary>
public class UniformGenerationStep : IGenerationStep
{
    public TerrainId TerrainId = TerrainId.VOID;

    public void Generate(TileMap tileMap, Vector2I topCorner, Vector2I bottomCorner)
    {
        var tiles = new Godot.Collections.Array<Vector2I>();

        for (int i = topCorner.X; i <= bottomCorner.X; i++)
        {
            for (int j = topCorner.Y; j <= bottomCorner.Y; j++)
            {
                tiles.Add(new(i, j));
            }
        }

        tileMap.SetCellsTerrainConnect(0, tiles, 0, (int)TerrainId);
    }
}