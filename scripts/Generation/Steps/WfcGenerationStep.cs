using Godot;
using ProjectMatchstick.Generation.Shapes;
using ProjectMatchstick.Generation.Steps;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMatchstick.Generation.Steps;

/// <summary>
/// Required the cells to fill are not empty or else an exception will be thrown.
/// </summary>
public class WfcGenerationStep : IGenerationStep
{
    private TerrainId FallbackTerrain { get; }
    private Random Random { get; }
    private HashSet<TerrainId> BackgroundTerrains { get; }

    private Dictionary<TerrainId, List<TerrainRule>> RuleSet = new()
    {
        { TerrainId.VOID, new List<TerrainRule> { new(TerrainId.WATER, 0.3), new(TerrainId.LAND, 0.3), new(TerrainId.WALL, 0.3) } },
        { TerrainId.WATER, new List<TerrainRule> { new(TerrainId.WATER, 2.0), new(TerrainId.LAND, 0.25) } },
        { TerrainId.LAND, new List<TerrainRule> { new(TerrainId.WATER, 0.5), new(TerrainId.LAND, 6.0), new(TerrainId.WALL, 1.0) } },
        { TerrainId.WALL, new List<TerrainRule> { new(TerrainId.LAND, 0.5), new(TerrainId.WALL, 2.0) } } 
    };

    public WfcGenerationStep(Dictionary<TerrainId, List<TerrainRule>> ruleSet, int seed, TerrainId fallbackTerrain, HashSet<TerrainId> backgroundTerrains)
    {
        // RuleSet = ruleSet;
        Random = new Random(seed);
        FallbackTerrain = fallbackTerrain;
        BackgroundTerrains = backgroundTerrains;
    }

    public void Generate(TileMap tileMap, IShape shape, GenerationRenderMode mode)
    {
        Generate(tileMap, shape.ToList(), mode);
    }

    public void Generate(TileMap tileMap, List<Vector2I> targetCellsList, GenerationRenderMode mode)
    {
        /*
         * terrainMap: the current state of the level
         * frontier: the list of cells which WFC will analyze for chaos, then pick the least chaotic cell
         * targetCells: the set of cells which are available to fill but haven't yet
         */

        var terrainMap = targetCellsList
            .ToDictionary(
                vec => vec,
                vec => (tileMap.GetCellSourceId(0, vec) == -1) ? TerrainId.NOTHING : (TerrainId)tileMap.GetCellTileData(0, vec).Terrain);
        var targetCellsSet = new HashSet<Vector2I>(targetCellsList);
        var frontier = InitializeFrontier(tileMap, terrainMap, targetCellsList, targetCellsSet);

        while (frontier.Count > 0)
        {
            (Vector2I cell, List<TerrainId> allowedTerrains, LinkedListNode<Vector2I> node) = GetLeastChaoticCell(tileMap, terrainMap, frontier);
            TerrainId selectedTerrarin = SelectRandomTerrain(tileMap, terrainMap, cell, allowedTerrains);

            if (mode == GenerationRenderMode.IMMEDIATE)
            {
                tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { cell }, 0, (int)selectedTerrarin);
            }

            terrainMap[cell] = selectedTerrarin;
            ExpandFrontier(ref frontier, tileMap, cell, terrainMap, targetCellsSet);

            frontier.Remove(node);
            targetCellsSet.Remove(cell);
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

    private LinkedList<Vector2I> InitializeFrontier(TileMap tileMap, Dictionary<Vector2I, TerrainId> terrainMap, List<Vector2I> targetCellsList, HashSet<Vector2I> targetCellsSet)
    {
        var frontier = new LinkedList<Vector2I>();

        foreach (Vector2I cell in targetCellsList)
        {
            var isFilled = terrainMap.TryGetValue(cell, out TerrainId thisTerrain);
            if (!isFilled || BackgroundTerrains.Contains(thisTerrain))
            {
                continue;
            }

            foreach (var neighbor in tileMap.GetSurroundingCells(cell))
            {
                terrainMap.TryGetValue(neighbor, out var neighborTerrain);
                if (targetCellsSet.Contains(neighbor) && BackgroundTerrains.TryGetValue(neighborTerrain, out var _))
                {
                    frontier.AddFirst(neighbor);
                }
            }

            targetCellsSet.Remove(cell);
        }

        return frontier;
    }

    private void ExpandFrontier(ref LinkedList<Vector2I> frontier, TileMap tileMap, Vector2I cell, Dictionary<Vector2I, TerrainId> terrainMap, HashSet<Vector2I> targetCells)
    {
        foreach (var neighbor in tileMap.GetSurroundingCells(cell))
        {
            terrainMap.TryGetValue(neighbor, out var terrain);

            if (BackgroundTerrains.Contains(terrain) && targetCells.Contains(neighbor) && !frontier.Contains(neighbor))
            {
                frontier.AddLast(neighbor);
            }
        }

        targetCells.Remove(cell);
    }

    private (Vector2I, List<TerrainId>, LinkedListNode<Vector2I>) GetLeastChaoticCell(TileMap tileMap, Dictionary<Vector2I, TerrainId> terrainMap, LinkedList<Vector2I> frontier)
    {
        var leastChaoticCell = Vector2I.Zero;
        var leastChaoticCellNode = new LinkedListNode<Vector2I>(new(0, 0));
        List<TerrainId> allowedTerrains = null;

        var node = frontier.First;

        while (node != null)
        {
            var cell = node.Value;

            List<TerrainId> candidateAllowedTerrains = GetAllowedTerrrains(tileMap, terrainMap, cell);

            if (candidateAllowedTerrains != null && candidateAllowedTerrains.Count == 0)
            {
                GD.Print($"Cell {cell} not allowed to have any neighbors.");
                continue;
            }

            if (allowedTerrains == null || candidateAllowedTerrains.Count < allowedTerrains.Count)
            {
                leastChaoticCell = cell;
                leastChaoticCellNode = node;
                allowedTerrains = candidateAllowedTerrains;
            }

            node = node.Next;
        }

        return (leastChaoticCell, allowedTerrains ?? new(), leastChaoticCellNode);
    }

    private List<TerrainId> GetAllowedTerrrains(TileMap tileMap, Dictionary<Vector2I, TerrainId> terrainMap, Vector2I cell)
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

        var terrainsAllowedByAllNeighbors = new List<TerrainId>();

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
        
        var sumWeights = new double[(int)Enum.GetValues(typeof(TerrainId)).Cast<TerrainId>().Max() + 1];
        
        foreach (var terrainId in allowedTerrains)
        {
            foreach (var neighborTerrainId in tileMap.GetSurroundingCells(cell)
                .Where(neighbor => terrainMap.ContainsKey(neighbor))
                .Select(neighbor => terrainMap[neighbor]))
            {
                sumWeights[(int)terrainId] += RuleSet[neighborTerrainId].Where(nid => nid.NeighborTerrainId == terrainId).FirstOrDefault().Weight;
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
