using Godot;
using ProjectMatchstick.Generation.Steps;
using System;

namespace ProjectMatchstick.Generation.Strategies;

public class ChanceMapGenerator : IGenerationStep
{
    private Random Random { get; }
    private double Chance { get; }

    public ChanceMapGenerator(double chance, int seed)
    {
        Random = new Random(seed);
        Chance = chance;
    }

    public void Generate(TileMap tileMap, Vector2I topCorner, Vector2I bottomCorner)
    {
        for (int i = topCorner.X; i < bottomCorner.X; i++)
        {
            for (int j = topCorner.Y; j < bottomCorner.Y; j++)
            {
                if (Random.NextDouble() < Chance)
                {
                    int terrain = Random.Next(tileMap.TileSet.GetTerrainsCount(0));
                    tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { new(i, j) }, 0, terrain);
                }
            }
        }
    }
}