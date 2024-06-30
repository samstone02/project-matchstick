using Godot;
using ProjectMatchstick.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ProjectMatchstick.Services.Generation.Steps.OverlappedWfcGenerationStep;

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
        public bool IsCollapsed { get => Terrain != -1; }
    }

    public class SequenceStep
    {
        public Pattern Pattern { get; set; }
        public Vector2I Position { get; set; }
        public List<Vector2I> Cells { get; set; }
        public List<(Pattern, Vector2I)> TriedPatterns { get; set; }
    }
    
    public int PatternSize { get; set; }

    public int[,] Sample { get; set; }

    public List<Vector2I> Generate(TileMap tileMap, List<Vector2I> targetCells, GenerationRenderMode mode)
    {
        var uniquePatterns = ExtractUniquePatterns();
        var map = InitializeMap(tileMap, targetCells, uniquePatterns);
        var frontier = InitializeFrontier(tileMap, map, uniquePatterns);
        var frontierTracker = new HashSet<Vector2I>();
        var sequeunce = new Stack<SequenceStep>();

        while (frontier.Count > 0)
        {
            var candidatePosition = frontier.Dequeue();

            if (map.TryGetValue(candidatePosition, out var value) && value.IsCollapsed)
            {
                /* Possible that cells in the frontier get collapsed since a pattern covers multiple cells. */
                /* Could also be enqueued with a different chaos value. */
                /* Check that the cell is not already collapsed before proceeding. */
                continue;
            }

            var pattern = SelectPattern(map, uniquePatterns, candidatePosition, sequeunce);

            if (pattern.Item1 == null)
            {
                var previousStep = sequeunce.Pop();

                foreach (var cell in previousStep.Cells)
                {
                    int chaos = GetChaosValue(map, tileMap, cell, uniquePatterns);

                    map[cell].Chaos = chaos;
                    map[cell].Terrain = 0;

                    frontier.Enqueue(cell, chaos);
                }

                var previousStep2 = sequeunce.Peek();
                previousStep2.TriedPatterns ??= new List<(Pattern, Vector2I)>();
                previousStep2.TriedPatterns.Add((previousStep.Pattern, previousStep.Position));

                continue;
            }

            var sequenceStep = ApplyPatternAt(map, pattern.Item1, pattern.Item2);
            
            sequeunce.Push(sequenceStep);

            foreach (var collapsedCell in sequenceStep.Cells)
            {
                foreach (var neighbor in tileMap.GetSurroundingCells(collapsedCell))
                {
                    var found = map.TryGetValue(neighbor, out var cell);

                    if (!map.TryGetValue(neighbor, out var value2) || value2.IsCollapsed || frontierTracker.Contains(neighbor))
                    {
                        continue;
                    }

                    frontier.Enqueue(neighbor, GetChaosValue(map, tileMap, neighbor, uniquePatterns));
                    frontierTracker.Add(neighbor);
                }
            }

            foreach (var group in sequenceStep.Cells.GroupBy(c => map[c].Terrain))
            {
                tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I>(group.ToList()), 0, group.Key);
            }
        }

        return targetCells; // TODO: return something else...
    }

    public Dictionary<Vector2I, Cell> InitializeMap(TileMap tileMap, List<Vector2I> targetCells, List<Pattern> uniquePatterns)
    {
        var map = new Dictionary<Vector2I, Cell>();

        foreach (var cell in targetCells)
        {
            map[cell] = new Cell
            {
                Terrain = -1,
                Chaos = int.MaxValue
            };
        }

        var usedCells = tileMap.GetUsedCells(0);

        foreach (var cell in usedCells)
        {
            map[cell] = new Cell
            {
                Terrain = tileMap.GetCellTileData(0, cell).Terrain
            };
        }

        foreach (var cell in usedCells)
        {
            map[cell].Chaos = GetChaosValue(map, tileMap, cell, uniquePatterns);
        }

        return map;
    }

    public PriorityQueue<Vector2I, int> InitializeFrontier(TileMap tileMap, Dictionary<Vector2I, Cell> map, List<Pattern> uniquePatterns)
    {
        var frontier = new PriorityQueue<Vector2I, int>();

        foreach (var cell in tileMap.GetUsedCells(0))
        {
            foreach (var neighbor in tileMap.GetSurroundingCells(cell))
            {
                if (map.TryGetValue(neighbor, out var value) && !value.IsCollapsed)
                {
                    frontier.Enqueue(neighbor, GetChaosValue(map, tileMap, neighbor, uniquePatterns));
                }
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
    public (Pattern, Vector2I) SelectPattern(Dictionary<Vector2I, Cell> map, List<Pattern> uniquePatterns, Vector2I candidatePosition, Stack<SequenceStep> sequence)
    {
        var validPatternsAndPositions = new List<(Pattern, Vector2I)>();

        foreach (var pattern in uniquePatterns)
        {
            foreach (var patternCellPosition in pattern.Cells.Keys)
            {
                var patternPosition = candidatePosition - patternCellPosition;
                var wasPatternTriedPreviously =
                    sequence.TryPeek(out var value)
                    && value.TriedPatterns != null
                    && value.TriedPatterns.Contains((pattern, patternPosition));

                if (CanApplyPatternAt(map, pattern, patternPosition) && !wasPatternTriedPreviously)
                {
                    validPatternsAndPositions.Add((pattern, patternPosition));
                }
            }
        }

        /* Select a random pattern + position weighted by the pattern's frequency */

        if (validPatternsAndPositions.Count == 0)
        {
            return (null, new(0,0));
        }

        var idx = RandomHelper.SelectRandomWeighted(validPatternsAndPositions, pat => pat.Item1.Frequency, new Random());
        
        return validPatternsAndPositions[idx];
    }

    public bool CanApplyPatternAt(Dictionary<Vector2I, Cell> map, Pattern pattern, Vector2I patternPosition)
    {
        int overlaps = 0;

        foreach (var cellPosition in pattern.Cells.Keys)
        {
            if (map.TryGetValue(patternPosition + cellPosition, out var value) && value.IsCollapsed)
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

    public SequenceStep ApplyPatternAt(Dictionary<Vector2I, Cell> map, Pattern pattern, Vector2I patternPosition)
    {
        var appliedCells = new List<Vector2I>();

        foreach (var patternCellPosition in pattern.Cells.Keys)
        {
            var mapCellPosition = patternCellPosition + patternPosition;

            if (!map.TryGetValue(mapCellPosition, out var value))
            {
                continue;
            }

            if (!value.IsCollapsed)
            {
                map[mapCellPosition] = new Cell
                {
                    Terrain = pattern.Cells[patternCellPosition]
                };
                appliedCells.Add(mapCellPosition);
            }
        }

        return new SequenceStep()
        {
            Cells = appliedCells,
            Position = patternPosition,
            Pattern = pattern
        };
    }

    public int GetChaosValue(Dictionary<Vector2I, Cell> map, TileMap tileMap, Vector2I cellPosition, List<Pattern> uniquePatterns)
    {
        int chaos = 0;

        foreach (var neighbor in tileMap.GetSurroundingCells(cellPosition)
                .Where(n => map.ContainsKey(n)))
        {
            if (!map.TryGetValue(neighbor, out var value) || value.IsCollapsed)
            {
                continue;
            }

            foreach (var pattern in uniquePatterns)
            {
                foreach (var patternCellPosition in pattern.Cells.Keys)
                {
                    if (CanApplyPatternAt(map, pattern, patternCellPosition))
                    {
                        chaos++;
                    }
                }
            }
        }

        return chaos;
    }
}
