using Godot;
using ProjectMatchstick.Generation.Steps;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMatchstick.Generation.Strategies;

public class WfcGenerationStep : IGenerationStep
{
    public struct TerrainRule
    {
        public TerrainId NeighborTerrainId;
        public double Weight;

        public TerrainRule(TerrainId neighborTerrainId, double percentage)
        {
            NeighborTerrainId = neighborTerrainId;
            Weight = percentage;
        }
    }

    private TerrainId FallbackTerrain { get; }

    private Random Random { get; }

    private Dictionary<TerrainId, List<TerrainRule>> RuleSet = new()
    {
        { TerrainId.VOID, new List<TerrainRule> { new(TerrainId.WATER, 0.33), new(TerrainId.LAND, 0.33), new(TerrainId.WALL, 0.33) } },
        { TerrainId.WATER, new List<TerrainRule> { new(TerrainId.WATER, 0.75), new(TerrainId.LAND, 0.1) } },
        { TerrainId.LAND, new List<TerrainRule> { new(TerrainId.WATER, 0.25), new(TerrainId.LAND, 1.0), new(TerrainId.WALL, 0.5) } },
        { TerrainId.WALL, new List<TerrainRule> { new(TerrainId.LAND, 0.25), new(TerrainId.WALL, 0.75) } } 
    };

    public WfcGenerationStep(List<List<TerrainRule>> ruleSet, int seed, TerrainId fallbackTerrain)
    {
        Random = new Random(seed);
        FallbackTerrain = fallbackTerrain;
    }

    public void Generate(TileMap tileMap, Vector2I topCorner, Vector2I bottomCorner)
    {
        List<Vector2I> voidCells = tileMap.GetUsedCells(0).Where(vec => (TerrainId)tileMap.GetCellTileData(0, vec).Terrain == TerrainId.VOID).ToList();

        var terrainMap = new TerrainId[Math.Abs(topCorner.X) + Math.Abs(bottomCorner.X) + 1, Math.Abs(topCorner.Y) + Math.Abs(bottomCorner.Y) + 1];

        while (voidCells.Count > 0)
        {
            (Vector2I cell, List<TerrainId> allowedTerrains) = GetLeastChaoticCell(tileMap, terrainMap, voidCells, topCorner, bottomCorner);

            TerrainId selectedTerrarin = SelectRandomTerrain(tileMap, cell, allowedTerrains);

            terrainMap[cell.X - topCorner.X, cell.Y - topCorner.Y] = selectedTerrarin;

            voidCells.Remove(cell);
        }

        var terrainsToSet = GetTerrainsDictionary(terrainMap, topCorner);

        foreach (var kv in terrainsToSet)
        {
            tileMap.SetCellsTerrainConnect(0, kv.Value, 0, (int)kv.Key);
        }
    }

    private Dictionary<TerrainId, Godot.Collections.Array<Vector2I>> GetTerrainsDictionary(TerrainId[,] terrainMap, Vector2I topCorner)
    {
        var terrainsToSet = new Dictionary<TerrainId, Godot.Collections.Array<Vector2I>>();

        for (int i = 0; i < terrainMap.GetLength(0); i++)
        {
            for (int j = 0; j < terrainMap.GetLength(1); j++)
            {
                if (!terrainsToSet.TryGetValue(terrainMap[i,j], out var _))
                {
                    terrainsToSet[terrainMap[i, j]] = new();
                }

                terrainsToSet[terrainMap[i, j]].Add(new Vector2I(i + topCorner.X, j + topCorner.Y));
            }
        }

        return terrainsToSet;
    }

    private (Vector2I, List<TerrainId>) GetLeastChaoticCell(TileMap tileMap, TerrainId[,] terrainMap, List<Vector2I> emptyCells, Vector2I topCorner, Vector2I bottomCorner)
    {
        var leastChaoticCell = Vector2I.Zero;

        HashSet<TerrainId> allowedTerrains = null;

        foreach (Vector2I cell in emptyCells)
        {
            HashSet<TerrainId> candidateAllowedTerrains = GetAllowedTerrrains(tileMap, terrainMap, cell, topCorner, bottomCorner);

            if (candidateAllowedTerrains != null && candidateAllowedTerrains.Count == 0)
            {
                GD.Print($"Cell {cell} not allowed to have any neighbors.");
                continue;
            }

            if (allowedTerrains == null || candidateAllowedTerrains.Count < allowedTerrains.Count)
            {
                leastChaoticCell = cell;
                allowedTerrains = candidateAllowedTerrains;
            }
        }

        return (leastChaoticCell, (allowedTerrains == null)? new() : allowedTerrains.ToList());
    }

    private HashSet<TerrainId> GetAllowedTerrrains(TileMap tileMap, TerrainId[,] terrainMap, Vector2I candidate, Vector2I topCorner, Vector2I bottomCorner)
    {
        var countsTerrainsAllowedByNeighbors = new Dictionary<TerrainId, int>(); // value = numberOfNeighborsWhichAllowThisTerrain

        int numNeighbors = 0;

        for (int i = Math.Max(0, candidate.X - topCorner.X - 1); i <= Math.Min(terrainMap.GetLength(0) - 1, candidate.X - topCorner.X + 1); i++)
        {
            for (int j = Math.Max(0, candidate.Y - topCorner.Y - 1); j <= Math.Min(terrainMap.GetLength(1) - 1, candidate.Y - topCorner.Y + 1); j++)
            {
                if (!IsNeighbor(tileMap, i, j, candidate - topCorner))
                {
                    continue;
                }

                numNeighbors++;

                if (terrainMap[i,j] == TerrainId.VOID)
                {
                    foreach (var terrainId in RuleSet.Keys)
                    {
                        if (terrainId == TerrainId.VOID)
                        {
                            continue;
                        }

                        if (countsTerrainsAllowedByNeighbors.TryGetValue(terrainId, out var _))
                        {
                            countsTerrainsAllowedByNeighbors[terrainId]++;
                        }
                        else
                        {
                            countsTerrainsAllowedByNeighbors[terrainId] = 1;
                        }
                    }

                    continue;
                }

                var terrainsAllowedByNeighbor = RuleSet[terrainMap[i,j]];

                foreach (TerrainRule terrainRule in terrainsAllowedByNeighbor)
                {
                    if (countsTerrainsAllowedByNeighbors.TryGetValue(terrainRule.NeighborTerrainId, out var _))
                    {
                        countsTerrainsAllowedByNeighbors[terrainRule.NeighborTerrainId]++;
                    }
                    else
                    {
                        countsTerrainsAllowedByNeighbors[terrainRule.NeighborTerrainId] = 1;
                    }
                }
            }
        }

        var terrainsAllowedByAllNeighbors = new HashSet<TerrainId>();

        foreach (var terrainCount in countsTerrainsAllowedByNeighbors)
        {
            if (terrainCount.Value == numNeighbors)
            {
                terrainsAllowedByAllNeighbors.Add(terrainCount.Key);
            }
        }

        return terrainsAllowedByAllNeighbors;
    }

    private bool IsNeighbor(TileMap tileMap, int x, int y, Vector2I candidate)
    {
        static bool IsCorner(int x, int y, Vector2I candidate)
            => x != candidate.X && y != candidate.Y;

        static bool IsSide(int x, int y, Vector2I candidate)
            => x == candidate.X ^ y == candidate.Y;
        
        if (x == candidate.X && y == candidate.Y)
        {
            return false;
        }

        if (tileMap.TileSet.GetTerrainSetMode(0) == TileSet.TerrainMode.Sides && IsCorner(x, y, candidate))
        {
            return false;
        }

        if (tileMap.TileSet.GetTerrainSetMode(0) == TileSet.TerrainMode.Corners && IsSide(x, y, candidate))
        {
            return false;
        }

        return true;
    }

    private TerrainId SelectRandomTerrain(TileMap tileMap, Vector2I cell, List<TerrainId> allowedTerrains)
    {
        if (allowedTerrains.Count == 0)
        {
            GD.Print($"No terrain is allowed at: {cell}. Falling back to {nameof(FallbackTerrain)}");
            return FallbackTerrain;
        }

        /* Get allowed neighboring tile's sum weight */
        var sumWeights = new double[allowedTerrains.Count];
        foreach (var terrainId in allowedTerrains)
        {
            foreach (var neighborId in tileMap.GetSurroundingCells(cell).Select(coords => tileMap.GetCellSourceId(0, coords)).Where(id => id > 0))
            {
                sumWeights[(int)terrainId] += RuleSet[(TerrainId)neighborId][(int)terrainId].Weight;
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
            return (TerrainId)allowedTerrains[Random.Next(allowedTerrains.Count)];
        }

        double r = Random.Next() * prefixSum[prefixSum.Length - 1];
        for (int i = 0; i < prefixSum.Length; i++)
        {
            if (prefixSum[i] > r)
            {
                return (TerrainId)allowedTerrains[i];
            }
        }

        throw new Exception($"Something went wrong generating a random number. allowedNeighbors={allowedTerrains}, prefixSum={prefixSum}, r={r}");
    }
}
