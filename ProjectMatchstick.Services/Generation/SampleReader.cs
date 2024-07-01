using Godot;
using System.Collections.Generic;

namespace ProjectMatchstick.Services.Generation;

public static class SampleReader
{
    public static Dictionary<Vector2I, int> SampleFromTileMap(TileMap tileMap)
    {
        var sample = new Dictionary<Vector2I, int>();

        foreach (var cell in tileMap.GetUsedCells(0))
        {
            sample.Add(cell, tileMap.GetCellTileData(0, cell).Terrain);
        }

        return sample;
    }
}
