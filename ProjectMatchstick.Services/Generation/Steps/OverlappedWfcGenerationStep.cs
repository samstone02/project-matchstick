using Godot;
using ProjectMatchstick.Services.Generation.PatternShapes;
using ProjectMatchstick.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMatchstick.Services.Generation.Steps;

// TODO: It looks like cells on the edge of the sample are being misread as able to be next to anything.
//      This is not how it should work, make it so cells on the border can only be next to cells next to them in the sample.
// TODO: Fix the empty stack / sequence issue. Sometimes throws an exception on Peek.
//      Probably because the stack size = 1, then we Pop, then we Peek and now empty stack.
// TODO: Fix the missing cells issue. Sometimes cells will simply be skipped. Don't know why.
// TODO: On an "dead end" (when no pattern can be applied), maybe we could end the algo early? Or add an option for that?
// TODO: There is a scenario where tiles that don't appear next to eachother in the sample can be placed next to eachother.
//      This happens when a Pattern is applied in a valid spot, but an immediate neighbor to one of the applied cells is not valid accoring to the sample.
//      This is lower priority, I think. Might also be fixed by fixing the sequeunce?
public class OverlappedWfcGenerationStep : IGenerationStep
{
    public class Pattern
    {
        public Dictionary<Vector2I, Cell> Cells { get; set; }
        public int Frequency { get; set; }

        public override int GetHashCode()
        {
            return Cells.Sum(kv => kv.Value.Terrain);
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
        public bool IsRotatable { get; set; }
    }

    public class SequenceStep
    {
        public Pattern Pattern { get; set; }
        public Vector2I Position { get; set; }
        public List<Vector2I> Cells { get; set; }
        public List<(Pattern, Vector2I)> TriedPatterns { get; set; }
    }

    public IPatternShape PatternShape { get; set; }
    
    public int PatternSize { get; set; }

    public Dictionary<Vector2I, Cell> Sample { get; set; }

    public Random Random { get; set; } = new Random();

    public int EmptyNeighborChaosBias { get; set; } = 100;

    public List<Vector2I> Generate(TileMap tileMap, List<Vector2I> targetCells, GenerationRenderMode mode)
    {
        List<Pattern> uniquePatterns = ExtractUniquePatterns(tileMap);
        Dictionary<Vector2I, Cell> map = InitializeMap(tileMap, targetCells, uniquePatterns);
        PriorityQueue<Vector2I, int> frontier = InitializeFrontier(tileMap, map, uniquePatterns);

        var frontierTracker = new HashSet<Vector2I>();
        var sequeunce = new Stack<SequenceStep>();

        while (frontier.Count > 0)
        {
            Vector2I candidatePosition = frontier.Dequeue();

            if (map.TryGetValue(candidatePosition, out var value) && value.IsCollapsed)
            {
                /* Possible that cells in the frontier get collapsed since a pattern covers multiple cells. */
                /* Could also be enqueued with a different chaos value. */
                /* Check that the cell is not already collapsed before proceeding. */
                continue;
            }

            // :/
            
            (Pattern pattern, Vector2I patternPosition) = SelectPattern(map, uniquePatterns, candidatePosition, sequeunce);

            if (pattern == null)
            {
                /* No valid pattern found at this position. Undo the last step and try a different step. */

                SequenceStep previousStep = sequeunce.Pop();

                foreach (var cell in previousStep.Cells)
                {
                    int chaos = GetChaosValue(map, tileMap, cell, uniquePatterns);

                    map.Remove(cell);

                    frontier.Enqueue(cell, chaos);
                }

                SequenceStep previousStep2 = sequeunce.Peek(); // TODO: Sometimes this throws an exception (empty stack), especially for PatternSize = 3. Figure out why.
                previousStep2.TriedPatterns ??= new List<(Pattern, Vector2I)>();
                previousStep2.TriedPatterns.Add((previousStep.Pattern, previousStep.Position));

                continue;
            }

            SequenceStep sequenceStep = ApplyPatternAt(map, pattern, patternPosition);
            
            sequeunce.Push(sequenceStep);

            foreach (Vector2I collapsedCell in sequenceStep.Cells)
            {
                foreach (Vector2I neighbor in tileMap.GetSurroundingCells(collapsedCell))
                {
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

        return targetCells; // TODO: Return a list of the unfilled cells
    }

    /// <summary>
    /// Initialize a dictionary representing the state of the world.
    /// </summary>
    public Dictionary<Vector2I, Cell> InitializeMap(TileMap tileMap, List<Vector2I> targetCells, List<Pattern> uniquePatterns)
    {
        var map = new Dictionary<Vector2I, Cell>();

        foreach (Vector2I cell in targetCells)
        {
            map[cell] = new Cell
            {
                Terrain = -1,
                Chaos = int.MaxValue
            };
        }

        Godot.Collections.Array<Vector2I> usedCells = tileMap.GetUsedCells(0);

        foreach (Vector2I cell in usedCells)
        {
            map[cell] = new Cell
            {
                Terrain = tileMap.GetCellTileData(0, cell).Terrain
            };
        }

        foreach (Vector2I cell in usedCells)
        {
            map[cell].Chaos = GetChaosValue(map, tileMap, cell, uniquePatterns);
        }

        return map;
    }

    /// <summary>
    /// Add intial positions to the frontier. The frontier exists for optimization purposes.
    /// </summary>
    public PriorityQueue<Vector2I, int> InitializeFrontier(TileMap tileMap, Dictionary<Vector2I, Cell> map, List<Pattern> uniquePatterns)
    {
        var frontier = new PriorityQueue<Vector2I, int>();

        foreach (Vector2I cell in tileMap.GetUsedCells(0))
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

    /// <summary>
    /// Extract evey unique pattern from the sample. Depending on the PatternShape, it will include rotations.
    /// If the same pattern is encountered twice, the frequency is incremented.
    /// </summary>
    public List<Pattern> ExtractUniquePatterns(TileMap tileMap)
    {
        var patterns = new List<Pattern>();
        var patternsTracker = new Dictionary<Pattern, Pattern>(); /* Using a Dictionary instead of HashMap becayse HashMaps suck. */

        foreach (Vector2I position in Sample.Keys)
        {
            Dictionary<Vector2I, Cell> unrotatedPattern = MapHelper.GetSubmap(Sample, PatternShape.Cells, position);
            var patternRotations = new List<Pattern>();

            if (unrotatedPattern == null)
            {
                continue;
            }

            bool doNotRotate = unrotatedPattern.Any(kv => !kv.Value.IsRotatable);

            if (doNotRotate)
            {
                patternRotations.Add(new()
                {
                    Cells = PatternShape.RotatePattern(unrotatedPattern, 0)
                });
            }

            else
            {
                foreach (int angle in PatternShape.SuperimposedRotations)
                {
                    patternRotations.Add(new()
                    {
                        Cells = PatternShape.RotatePattern(unrotatedPattern, angle)
                    });
                }
            }

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
                
        return patterns;
    }

    /// <summary>
    /// Select a single pattern at a position to apply. The pattern will overlap the least chaotic cell and overlap at least one collapsed cell.
    /// The position is the corner cell corresponding to the min X and min Y of the pattern.
    /// </summary>
    public (Pattern, Vector2I) SelectPattern(Dictionary<Vector2I, Cell> map, List<Pattern> uniquePatterns, Vector2I candidatePosition, Stack<SequenceStep> sequence)
    {
        var validPatternsAndPositions = new List<(Pattern, Vector2I)>();

        foreach (Pattern pattern in uniquePatterns)
        {
            foreach (Vector2I patternCellPosition in pattern.Cells.Keys)
            {
                Vector2I patternPosition = candidatePosition - patternCellPosition;
                bool wasPatternTriedPreviously =
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

        int idx = RandomHelper.SelectRandomWeighted(validPatternsAndPositions, pat => pat.Item1.Frequency, Random);
        
        return validPatternsAndPositions[idx];
    }

    /// <summary>
    /// Can the pattern at the position be applied? If the pattern correctly overlaps the map (any overlap has the same cell type) then true.
    /// </summary>
    public bool CanApplyPatternAt(Dictionary<Vector2I, Cell> map, Pattern pattern, Vector2I patternPosition)
    {
        int overlaps = 0;

        foreach (Vector2I cellPosition in pattern.Cells.Keys)
        {
            if (map.TryGetValue(patternPosition + cellPosition, out var value) && value.IsCollapsed)
            {
                if (value.Terrain == pattern.Cells[cellPosition].Terrain)
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

    /// <summary>
    /// Apply the pattern at the position. Assuming it is a valid position for the pattern.
    /// </summary>
    /// <returns>
    /// A SequenceStep containing the position of the pattern and the cells that were applied.
    /// </returns>
    public SequenceStep ApplyPatternAt(Dictionary<Vector2I, Cell> map, Pattern pattern, Vector2I patternPosition)
    {
        var appliedCells = new List<Vector2I>();

        foreach (Vector2I patternCellPosition in pattern.Cells.Keys)
        {
            var mapCellPosition = patternCellPosition + patternPosition;

            if (!map.TryGetValue(mapCellPosition, out var value))
            {
                continue;
            }

            if (!value.IsCollapsed)
            {
                map[mapCellPosition] = pattern.Cells[patternCellPosition];
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

    /// <summary>
    /// Gets the "chaos" of the cell. Chaos is essentially a count of how many patterns are valid given this cell and the state of the world.
    /// </summary>
    public int GetChaosValue(Dictionary<Vector2I, Cell> map, TileMap tileMap, Vector2I cellPosition, List<Pattern> uniquePatterns)
    {
        int chaos = 0;

        // TODO: Pick the neighbors based on the PatternShape

        foreach (var neighbor in tileMap.GetSurroundingCells(cellPosition)
            .Where(n => map.ContainsKey(n)))
        {
            //if (!map.TryGetValue(neighbor, out var value) || value.IsCollapsed)
            //{
            //    // TODO: Is there a better way to avoid the border problem than this bias?
            //    chaos += EmptyNeighborChaosBias;
            //    continue;
            //}

            foreach (var pattern in uniquePatterns)
            {
                foreach (var patternCellPosition in pattern.Cells.Keys)
                {
                    if (CanApplyPatternAt(map, pattern, cellPosition + patternCellPosition))
                    {
                        chaos++;
                    }
                }
            }
        }

        return chaos;
    }
}