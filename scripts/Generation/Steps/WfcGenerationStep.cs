using Godot;
using ProjectMatchstick.Generation.Steps;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMatchstick.scripts.Generation.Strategies;

public class WfcGenerationStep : IGenerationStep
{
    public struct TerrainRule
    {
        public int NeighborTerrainId;
        public double Weight;

        public TerrainRule(int neighborTerrainId, double percentage)
        {
            NeighborTerrainId = neighborTerrainId;
            Weight = percentage;
        }
    }

    private Random Random { get; }

    private List<List<TerrainRule>> RuleSet = new()
    {
        { new List<TerrainRule> { new(0, 0.75), new(1, 0.1) } }, // Water
        { new List<TerrainRule> { new(0, 0.25), new(1, 1.0), new(2, 0.5) } }, // Land
        { new List<TerrainRule> { new(1, 0.25), new(2, 0.75) } } // Cliff
    };

    public WfcGenerationStep(List<List<TerrainRule>> ruleSet, int seed)
    {
        Random = new Random(seed);
    }

    public void Generate(TileMap tileMap, Vector2I topCorner, Vector2I bottomCorner)
    {
        List<Vector2I> voidCells = tileMap.GetUsedCells(0).Where(vec => tileMap.GetCellTileData(0, vec).Terrain == TerrainIds.VOID).ToList();

        while (voidCells.Count > 0)
        {
            (Vector2I cell, List<int> allowedTerrains) = GetLeastChaoticCell(tileMap, voidCells, topCorner, bottomCorner);

            int selectedTerrarin = SelectRandomTerrain(tileMap, cell, allowedTerrains);

            tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { cell }, 0, selectedTerrarin); // fix

            voidCells.Remove(cell);
        }
    }

    private (Vector2I, List<int>) GetLeastChaoticCell(TileMap tileMap, List<Vector2I> emptyCells, Vector2I topCorner, Vector2I bottomCorner)
    {
        var leastChaoticCell = Vector2I.Zero;

        var allowedTerrains = new List<int>();

        foreach (Vector2I cell in emptyCells)
        {
            List<int> candidateAllowedTerrains = GetAllowedTerrrains(tileMap, cell, topCorner, bottomCorner);

            if (allowedTerrains.Count == 0 || candidateAllowedTerrains.Count < allowedTerrains.Count)
            {
                leastChaoticCell = cell;
                allowedTerrains = candidateAllowedTerrains;
            }
        }

        return (leastChaoticCell, allowedTerrains);
    }

    public List<int> GetAllowedTerrrains(TileMap tileMap, Vector2I candidate, Vector2I topCorner, Vector2I bottomCorner)
    {
        var numsAllowedBy = new int[RuleSet.Count]; // index = terrainSourceId, value = numberOfNeighborsWhichAllowThisTerrain

        foreach (Vector2I neighbor in tileMap.GetSurroundingCells(candidate))
        {
            if (neighbor.Y < topCorner.Y || neighbor.Y > bottomCorner.Y || neighbor.X < topCorner.X || neighbor.X > bottomCorner.X)
            {
                continue;
            }

            var neighborTerrainId = tileMap.GetCellTileData(0, neighbor).Terrain;

            if (neighborTerrainId == TerrainIds.VOID)
            {
                for (int i = 0; i < numsAllowedBy.Length; i++)
                {
                    numsAllowedBy[i]++;
                }

                continue;
            }

            var terrainsAllowedByNeighbor = RuleSet[neighborTerrainId];

            foreach (TerrainRule terrainRule in terrainsAllowedByNeighbor)
            {
                numsAllowedBy[terrainRule.NeighborTerrainId]++;
            }
        }

        var goodNeighbors = new List<int>();

        int numNeighbors = 4; // (tileMap.TileSet.GetTerrainSetMode(0) == TileSet.TerrainMode.CornersAndSides) ? 8 : 4; 

        for (int i = 0; i < numsAllowedBy.Length; i++)
        {
            if (numsAllowedBy[i] == numNeighbors)
            {
                goodNeighbors.Add(i);
            }
        }

        return goodNeighbors;
    }

    private int SelectRandomTerrain(TileMap tileMap, Vector2I cell, List<int> allowedTerrains)
    {
        if (allowedTerrains.Count == 0)
        {
            GD.Print("No terrain is allowed at: " + cell + ". Defaulting to Cliff.");
            return 2;
        }

        /* Get allowed neighboring tile's sum weight */
        var sumWeights = new double[allowedTerrains.Count];
        foreach (var terrainId in allowedTerrains)
        {
            foreach (var neighborId in tileMap.GetSurroundingCells(cell).Select(coords => tileMap.GetCellSourceId(0, coords)).Where(id => id > 0))
            {
                sumWeights[terrainId] += RuleSet[neighborId][terrainId].Weight;
            }
        }

        /* Get prefix sum from sumWeights */
        var prefixSum = new double[sumWeights.Length];
        for (int i = 0; i < prefixSum.Length; i++)
        {
            double previousSum = i == 0 ? 0 : prefixSum[i - 1];
            prefixSum[i] = sumWeights[i] + previousSum;
        }

        if (prefixSum[prefixSum.Length - 1] == 0)
        {
            return allowedTerrains[Random.Next(allowedTerrains.Count)];
        }

        double r = Random.Next() * prefixSum[prefixSum.Length - 1];
        for (int i = 0; i < prefixSum.Length; i++)
        {
            if (prefixSum[i] > r)
            {
                return allowedTerrains[i];
            }
        }

        throw new Exception($"Something went wrong generating a random number. allowedNeighbors={allowedTerrains}, prefixSum={prefixSum}, r={r}");
    }
}
