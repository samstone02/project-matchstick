using Godot;
using ProjectMatchstick.Generation.Shapes;
using ProjectMatchstick.Generation.Steps;
using System.Collections.Generic;

namespace ProjectMatchstick.Generation.Strategies;

/// <summary>
/// Mostly for preprocessing the TileMap. Prevents exceptions on `TileData.Terrain` by setting empty cells to a default terrain.
/// </summary>
public class UniformGenerationStep : IGenerationStep
{
    public TerrainId TerrainId = TerrainId.VOID;

    public void Generate(TileMap tileMap, IShape generationShape, GenerationRenderMode mode)
    {
        throw new System.NotImplementedException();
    }

    public void Generate(TileMap tileMap, List<Vector2I> cellsToFill, GenerationRenderMode mode)
    {
        var tiles = new Godot.Collections.Array<Vector2I>();

        foreach (var cell in cellsToFill)
        {
            if (mode == GenerationRenderMode.IMMEDIATE)
            {
                tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { cell }, 0, (int)TerrainId);
            }
            else
            {
                tiles.Add(cell);
            }
        }

        if (mode == GenerationRenderMode.ON_STEP_COMPLETE)
        {
            tileMap.SetCellsTerrainConnect(0, tiles, 0, (int)TerrainId);
        }
    }
}