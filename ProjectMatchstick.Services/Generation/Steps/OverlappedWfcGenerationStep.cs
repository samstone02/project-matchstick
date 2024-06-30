using Godot;
using ProjectMatchstick.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMatchstick.Services.Generation.Steps;

public struct OverlappedWfcGenerationStep : IGenerationStep
{
    public class Pattern
    {
        public Dictionary<Vector2I, int> Cells { get; set; }
        public int Frequency { get; set; }

        public override int GetHashCode()
        {
            return Cells.Sum(kv => kv.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Pattern other)
            {
                if (Cells.Count != other.Cells.Count)
                {
                    return false;
                }

                foreach (var key in Cells.Keys)
                {
                    var contains = other.Cells.TryGetValue(key, out var value);

                    if (!contains || Cells[key] != value)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }

    public class Cell
    {
        public int Terrain { get; set; }
        public int Chaos { get; set; }
        public bool IsCollapsed { get; set; }
    }

    public class SequenceStep
    {
        public List<Vector2I> Cells { get; set; }
        public List<Pattern> TriedPatterns { get; set; }
    }
    
    public int PatternSize { get; set; }

    public int[,] Sample {  get; set; }

    public List<Vector2I> Generate(TileMap tileMap, List<Vector2I> targetCells, GenerationRenderMode mode)
    {
        var map = InitializeMap(tileMap);
        var targetCellsSet = new HashSet<Vector2I>(targetCells);
        var frontier = InitializeFrontier(tileMap, map);
        var uniquePatterns = ExtractUniquePatterns();
        var sequeunce = new Stack<SequenceStep>();  // TODO: backtracking with sequences

        while (frontier.Count > 0)
        {
            var leastChaoticCellPosition = frontier.Dequeue();

            var pat = SelectPattern(map, targetCellsSet, frontier, uniquePatterns, leastChaoticCellPosition);
            var sequenceStep = ApplyPattern(map, pat.Item1, pat.Item2);
            //sequeunce.Push(sequenceStep);

            foreach (var collapsedCell in sequenceStep.Cells)
            {
                foreach (var neighbor in tileMap.GetSurroundingCells(collapsedCell))
                {
                    if (!map.ContainsKey(neighbor))
                    {
                        frontier.Enqueue(neighbor, GetChaosValue(map, tileMap, neighbor, uniquePatterns, targetCellsSet));
                    }
                }
            }
        }

        return targetCells;
    }

    public Dictionary<Vector2I, Cell> InitializeMap(TileMap tileMap)
    {
        var map = new Dictionary<Vector2I, Cell>();

        foreach (var cell in tileMap.GetUsedCells(0))
        {
            map.Add(cell, new Cell
            {
                IsCollapsed = true,
                Terrain = tileMap.GetCellTileData(0, cell).Terrain,
            });
        }

        return map;
    }

    // TODO: Initialize frontier with correct chaos values...
    public PriorityQueue<Vector2I, int> InitializeFrontier(TileMap tileMap, Dictionary<Vector2I, Cell> map)
    {
        var frontier = new PriorityQueue<Vector2I, int>();

        foreach (var cell in map)
        {
            foreach (var neighbor in tileMap.GetSurroundingCells(cell.Key))
            {
                frontier.Enqueue(neighbor, 1);
            }
        }

        return frontier;
    }

    public List<Pattern> ExtractUniquePatterns()
    {
        var patterns = new List<Pattern>();
        var patternsTracker = new Dictionary<Pattern, Pattern>(); // Keys reference the values cuz HashMaps suck
        
        var size = PatternSize switch
        {
            2 => 3,
            3 => 5,
            _ => throw new ArgumentException($"Pattern size {PatternSize} is not yet supported.")
        };

        for (var x = 0; x <= Sample.GetLength(0) - size; x++)
        {
            for (var y = 0; y <= Sample.GetLength(1) - size; y++)
            {
                var unrotatedPattern = MatrixHelper.GetSubmatrix(Sample, x, y, size);

                /*
                 * A (haxagonal) cell's neighbors:
                 * 
                 * X = The Cell
                 * N = A Neighbor
                 *   = Not a Neighbor
                 * 
                 * +-+-+-+-> Y
                 * | |N|N|
                 * +-+-+-+
                 * |N|X|N|
                 * +-+-+-+
                 * |N|N| |
                 * +-+-+-+
                 * |
                 * V X
                 * 
                 * In other words, for a 2x2 pattern, the cells at (x + 1, y + 1) and (x - 1, y - 1) are not neighbors.
                 * 
                 * For higher sizes apply this rule for the size down recursively.
                 * 
                 * Aside, a pattern's origin is always the cell with the most negative X and Y coordinates.
                 * For example: If the pattern is represented as a matrix (2d array), then the origin is [0,0].
                 * 
                 * In this function, a matrix value of -1 is assumed to be empty.
                 * 
                 * Due to limitations in my knowledge, patterns only support 90 degree rotations.
                 * This is because hexagons can't be rotated by 90 or 270 degrees and be superimposed.
                 * TODO: add more rotations (60, 120, 180, 240, 300, 360)
                 */

                if (size == 3)
                {
                    unrotatedPattern[0, 0] = -1;
                    unrotatedPattern[2, 2] = -1;
                }
                else if (size == 5)
                {
                    unrotatedPattern[0, 0] = -1;
                    unrotatedPattern[0, 1] = -1;
                    unrotatedPattern[1, 0] = -1;
                    unrotatedPattern[2, 2] = -1;
                    unrotatedPattern[1, 2] = -1;
                    unrotatedPattern[2, 1] = -1;
                }

                var patternRotations = new List<Pattern>
                {
                    new() { Cells = MatrixHelper.ToVectorDictionary(unrotatedPattern, val => val != -1) },
                    //new() { Cells = MatrixHelper.ToVectorDictionary(MatrixHelper.RotateClockwise(MatrixHelper.Clone(unrotatedPattern), 1), val => val != -1) },
                    new() { Cells = MatrixHelper.ToVectorDictionary(MatrixHelper.RotateClockwise(MatrixHelper.Clone(unrotatedPattern), 2), val => val != -1) },
                    //new() { Cells = MatrixHelper.ToVectorDictionary(MatrixHelper.RotateClockwise(MatrixHelper.Clone(unrotatedPattern), 3), val => val != -1) },
                };

                foreach (var pat in patternRotations)
                {
                    var isFound = patternsTracker.TryGetValue(pat, out var original);

                    if (isFound)
                    {
                        original.Frequency++;
                    }
                    else
                    {
                        pat.Frequency = 1;

                        patternsTracker[pat] = pat;
                        patterns.Add(pat);
                    }
                }
            }
        }

        return patterns;
    }

    /// <summary>
    /// Select a single pattern at a position to apply. The pattern will overlap the least chaotic cell and overlap at least one collapsed cell.
    /// The position is the corner cell corresponding to the min X and min Y of the pattern.
    /// </summary>
    public (Pattern, Vector2I) SelectPattern(
        Dictionary<Vector2I, Cell> map, HashSet<Vector2I> targetCellsSet, PriorityQueue<Vector2I, int> frontier,
        List<Pattern> uniquePatterns, Vector2I leastChaoticCellPosition)
    {
        var validPatternsAndPositions = new List<(Pattern, Vector2I)>();

        foreach (var pattern in uniquePatterns)
        {
            foreach (var patternCellPosition in pattern.Cells.Keys)
            {
                if (CanApplyPatternAt(map, targetCellsSet, pattern, leastChaoticCellPosition - patternCellPosition))
                {
                    validPatternsAndPositions.Add((pattern, leastChaoticCellPosition - patternCellPosition));
                }
            }
        }

        /* Select a random pattern + position weighted by the pattern's frequency */
        var idx = RandomHelper.SelectRandomWeighted(validPatternsAndPositions, pat => pat.Item1.Frequency, new Random());
        return validPatternsAndPositions[idx];
    }

    public bool CanApplyPatternAt(Dictionary<Vector2I, Cell> map, HashSet<Vector2I> targetCellsSet, Pattern pattern, Vector2I patternPosition)
    {
        int overlaps = 0;

        foreach (var cellPosition in pattern.Cells.Keys)
        {
            if (map.TryGetValue(patternPosition + cellPosition, out var value))
            {
                if (value.Terrain == pattern.Cells[cellPosition])
                {
                    overlaps++;
                }
                else
                {
                    return false;
                }
            }
        }

        return overlaps > 0;
    }

    public SequenceStep ApplyPattern(Dictionary<Vector2I, Cell> map, Pattern pattern, Vector2I patternPosition)
    {
        var appliedCells = new List<Vector2I>();

        foreach (var patternCellPosition in pattern.Cells.Keys)
        {
            var mapPosition = patternCellPosition + patternPosition;

            // NOTE: Might need to change this when we assign cell chaoses to the map!!!
            if (!map.ContainsKey(mapPosition))
            {
                map.Add(mapPosition, new Cell
                {
                    Terrain = pattern.Cells[patternCellPosition],
                    IsCollapsed = true
                });
                appliedCells.Add(mapPosition);
            }
        }

        return new SequenceStep() { Cells = appliedCells };
    }

    public int GetChaosValue(Dictionary<Vector2I, Cell> map, TileMap tileMap, Vector2I cellPosition, List<Pattern> uniquePatterns, HashSet<Vector2I> targetCellsSet)
    {
        int chaos = 0;

        foreach (var neighbor in tileMap.GetSurroundingCells(cellPosition)
                .Where(n => map.ContainsKey(n)))
        {
            if (!targetCellsSet.Contains(neighbor))
            {
                continue;
            }

            foreach (var pattern in uniquePatterns)
            {
                foreach (var patternCellPosition in pattern.Cells.Keys)
                {
                    if (CanApplyPatternAt(map, targetCellsSet, pattern, patternCellPosition))
                    {
                        chaos++;
                    }
                }
            }
        }

        return chaos;
    }
}
