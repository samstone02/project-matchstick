using Godot;
using ProjectMatchstick.Generation.Shapes;
using ProjectMatchstick.Generation.Steps;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMatchstick.Generation.Strategies;

/// <summary>
/// Required the cells to fill are not empty or else an exception will be thrown.
/// </summary>
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
        { TerrainId.VOID, new List<TerrainRule> { new(TerrainId.WATER, 0.3), new(TerrainId.LAND, 0.3), new(TerrainId.WALL, 0.3) } },
        { TerrainId.WATER, new List<TerrainRule> { new(TerrainId.WATER, 2.0), new(TerrainId.LAND, 0.25) } },
        { TerrainId.LAND, new List<TerrainRule> { new(TerrainId.WATER, 0.5), new(TerrainId.LAND, 6.0), new(TerrainId.WALL, 1.0) } },
        { TerrainId.WALL, new List<TerrainRule> { new(TerrainId.LAND, 0.5), new(TerrainId.WALL, 2.0) } } 
    };

    public WfcGenerationStep(List<List<TerrainRule>> ruleSet, int seed, TerrainId fallbackTerrain)
    {
        Random = new Random(seed);
        FallbackTerrain = fallbackTerrain;
    }

    public void Generate(TileMap tileMap, IShape shape, GenerationRenderMode mode)
    {
        Generate(tileMap, shape.ToList(), mode);
    }

    public void Generate(TileMap tileMap, List<Vector2I> cellsToFill, GenerationRenderMode mode)
    {
        // List<Vector2I> voidCells = tileMap.GetUsedCells(0).Where(vec => (TerrainId)tileMap.GetCellTileData(0, vec).Terrain == TerrainId.VOID).ToList();

        // var terrainMap = new TerrainId[Math.Abs(topCorner.X) + Math.Abs(bottomCorner.X) + 1, Math.Abs(topCorner.Y) + Math.Abs(bottomCorner.Y) + 1];

        var terrainMap = cellsToFill.ToDictionary(vec => vec, vec => (TerrainId)tileMap.GetCellTileData(0, vec).Terrain);

        while (cellsToFill.Count > 0)
        {
            (Vector2I cell, List<TerrainId> allowedTerrains) = GetLeastChaoticCell(tileMap, terrainMap, cellsToFill, tileMap.TileSet.GetTerrainSetMode(0));

            TerrainId selectedTerrarin = SelectRandomTerrain(tileMap, terrainMap, cell, allowedTerrains);

            cellsToFill.Remove(cell);

            terrainMap[cell] = selectedTerrarin;

            if (mode == GenerationRenderMode.IMMEDIATE)
            {
                tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { cell }, 0, (int)selectedTerrarin);
            }
        }

        if (mode == GenerationRenderMode.ON_STEP_COMPLETE)
        {
            var terrainsToSet = terrainMap.GroupBy(keySelector => keySelector.Value);

            foreach (var grouping in terrainsToSet)
            {
                var terrain = grouping.Select(kv => kv.Key);
                tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I>(terrain), 0, (int)grouping.Key);
            }
        }
    }

    private (Vector2I, List<TerrainId>) GetLeastChaoticCell(TileMap tileMap, Dictionary<Vector2I, TerrainId> terrainMap, List<Vector2I> cellsToFill, TileSet.TerrainMode terrainSetMode)
    {
        var leastChaoticCell = Vector2I.Zero;

        HashSet<TerrainId> allowedTerrains = null;

        foreach (Vector2I cell in cellsToFill)
        {
            HashSet<TerrainId> candidateAllowedTerrains = GetAllowedTerrrains(tileMap, terrainMap, cell, terrainSetMode);

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

    private HashSet<TerrainId> GetAllowedTerrrains(TileMap tileMap, Dictionary<Vector2I, TerrainId> terrainMap, Vector2I cell, TileSet.TerrainMode terrainSetMode)
    {
        IEnumerable<Vector2I> neighbors = tileMap.GetSurroundingCells(cell).Where(neighbor => terrainMap.TryGetValue(neighbor, out var _));
        var countsTerrainsAllowedByNeighbors = new Dictionary<TerrainId, int>(); // value = numberOfNeighborsWhichAllowThisTerrain
        int numNeighbors = 0;

        foreach (var neighbor in neighbors)
        {
            numNeighbors++;

            var terrainsAllowedByNeighbor = RuleSet[terrainMap[neighbor]];

            foreach (TerrainRule terrainRule in terrainsAllowedByNeighbor)
            {
                countsTerrainsAllowedByNeighbors.TryGetValue(terrainRule.NeighborTerrainId, out var count);
                countsTerrainsAllowedByNeighbors[terrainRule.NeighborTerrainId] = count + 1;
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

    private TerrainId SelectRandomTerrain(TileMap tileMap, Dictionary<Vector2I, TerrainId> terrainMap, Vector2I cell, List<TerrainId> allowedTerrains)
    {
        if (allowedTerrains.Count == 0)
        {
            GD.Print($"No terrain is allowed at: {cell}. Falling back to {nameof(FallbackTerrain)}");
            return FallbackTerrain;
        }

        /* Get allowed neighboring tile's sum weight */
        
        var sumWeights = new double[(int)Enum.GetValues(typeof(TerrainId)).Cast<TerrainId>().Last() + 1];
        
        foreach (var terrainId in allowedTerrains)
        {
            foreach (var neighborTerrainId in tileMap.GetSurroundingCells(cell)
                .Where(neighbor => terrainMap.TryGetValue(neighbor, out var _))
                .Select(neighbor => terrainMap[neighbor]))
            {
                sumWeights[(int)terrainId] += RuleSet[neighborTerrainId].Where(nid => nid.NeighborTerrainId == terrainId).FirstOrDefault().Weight;
            }
        }

        /* Get prefix sum from sumWeights */
        
        var prefixSum = new double[(int)Enum.GetValues(typeof(TerrainId)).Cast<TerrainId>().Last() + 1];
        for (int i = 0; i < prefixSum.Length; i++)
        {
            double previousSum = i == 0 ? 0 : prefixSum[i - 1];
            prefixSum[i] = sumWeights[i] + previousSum;
        }

        if (prefixSum[prefixSum.Length - 1] == 0)
        {
            return allowedTerrains[Random.Next(allowedTerrains.Count)];
        }

        /* Select random value from 0 to totalSum. Return the TerrainId which corresponds to this random value in the prefixSum  */

        double r = Random.NextDouble() * prefixSum[prefixSum.Length - 1];
        for (int i = 0; i < prefixSum.Length; i++)
        {
            if (prefixSum[i] > r)
            {
                return allowedTerrains.Where(t => t == (TerrainId)i).FirstOrDefault();
            }
        }

        throw new Exception($"Something went wrong generating a random number. allowedNeighbors={allowedTerrains}, prefixSum={prefixSum}, r={r}");
    }
}
