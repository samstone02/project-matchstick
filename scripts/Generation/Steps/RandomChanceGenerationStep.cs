using Godot;
using ProjectMatchstick.Generation.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMatchstick.Generation.Steps;

public class ChanceMapGenerator : IGenerationStep
{
    public Random Random { get; }
    public double Chance { get; }

    public void Generate(TileMap tileMap, IShape generationShape, GenerationRenderMode mode)
    {
        throw new NotImplementedException();
    }

    public void Generate(TileMap tileMap, List<Vector2I> cellsToFill, GenerationRenderMode mode)
    {
        var tiles = new Godot.Collections.Array<Vector2I>();

        var terrains = new Dictionary<int, Vector2I>();

        foreach (var cell in cellsToFill)
        {
            int terrain = 0; 
            if (Random.NextDouble() < Chance)
            {
                terrain = Random.Next(tileMap.TileSet.GetTerrainsCount(0));
            }

            if (mode == GenerationRenderMode.IMMEDIATE)
            {
                tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { cell }, 0, terrain);
            }
            else
            {
                terrains.Add(terrain, cell);
            }
        }

        if (mode == GenerationRenderMode.ON_STEP_COMPLETE)
        {
            foreach (var group in terrains.GroupBy(ter => ter.Key))
            {
                tileMap.SetCellsTerrainConnect(0, new(group.Select(g => g.Value)), 0, group.Key);
            }
        }
    }
}