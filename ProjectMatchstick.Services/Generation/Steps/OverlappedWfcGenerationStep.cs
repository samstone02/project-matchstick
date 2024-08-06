using Godot;
using ProjectMatchstick.Services.Generation.PatternShapes;
using ProjectMatchstick.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMatchstick.Services.Generation.Steps;

// TODO: How should we handle ChaosValues of zero? 
// TODO: Fix the empty stack / sequence issue. Sometimes throws an exception on Peek.
//      Probably because the stack size = 1, then we Pop, then we Peek and now empty stack.
// TODO: Fix the missing cells issue. Sometimes cells will simply be skipped. Don't know why.
//      - Something with the frontier and/or the sequeunce is causing this I think
// TODO: There is a scenario where tiles that don't appear next to eachother in the sample can be placed next to eachother.
//      This happens when a Pattern is applied in a valid spot, but an immediate neighbor to one of the applied cells is not valid accoring to the sample.
//      This is lower priority, I think. Might also be fixed by fixing the sequeunce?
public class OverlappedWfcGenerationStep : IGenerationStep
{
    private const int UNSET_TERRAIN = 0;

    private Dictionary<int, bool> PartiallyCollapsedRows { get; set; }

    private Dictionary<int, bool> PartiallyCollapsedColumns { get; set; }

    public class Pattern
    {
        public Dictionary<Vector2I, PatternCell> Cells { get; set; }
        public int Frequency { get; set; }

        public override int GetHashCode()
        {
            // TODO: Optimize hash codes to reduce collisions?
            return Cells.Sum(kv => kv.Value.Terrain * kv.Value.Terrain);
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

                    if (!contains || Cells[key].Terrain != value.Terrain)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }

    public class PatternCell
    {
        public int Terrain { get; set; }
        public bool IsRotatable { get; set; }
    }

    public class MapCell
    {
        public int Terrain { get; set; }
        //public int Chaos { get; set; }
        public bool IsCollapsed { get => Terrain != UNSET_TERRAIN; }
        //public bool IsFrontier {  get; set; }
    }

    public class SequenceStep
    {
        public Pattern Pattern { get; set; }
        public Vector2I Position { get; set; }
        public List<Vector2I> Cells { get; set; }
        /// <summary>
        /// The cells which were part of the frontier before apply the pattern.
        /// </summary>
        public List<Vector2I> FrontieredCells { get; set; }
        public List<(Pattern, Vector2I)> TriedPatterns { get; set; }
    }

    public IPatternShape PatternShape { get; set; }

    public Dictionary<Vector2I, PatternCell> Sample { get; set; }

    public Random Random { get; set; } = new Random();

    public int EmptyNeighborChaosBias { get; set; } = 100;

    public List<Vector2I> Generate(TileMap tileMap, List<Vector2I> targetCells, GenerationRenderMode mode)
    {
        List<Pattern> uniquePatterns = ExtractUniquePatterns();
        Dictionary<Vector2I, MapCell> map = InitializeMap(tileMap, targetCells, uniquePatterns);
        //PriorityQueue<Vector2I, int> frontier = InitializeFrontier(tileMap, map, uniquePatterns); // Should the frontier be something else to prevent duplicates?

        //var sequeunce = new Stack<SequenceStep>();
        //sequeunce.Push(new SequenceStep());

        //while (frontier.Count > 0)
        while (true)
        {
            //Vector2I candidatePosition = frontier.Dequeue();

            (Vector2I candidatePosition, bool isFound) = GetLeastChaoticPosition(tileMap, map, uniquePatterns);

            if (!isFound)
            {
                break;
            }

            if (map.TryGetValue(candidatePosition, out var value) && value.IsCollapsed)
            {
                /* Possible that cells in the frontier get collapsed since a pattern covers multiple cells. */
                /* Could also be enqueued with a different chaos value. */
                /* Check that the cell is not already collapsed before proceeding. */
                continue;
            }
            
            //(Pattern pattern, Vector2I patternPosition) = SelectPattern(tileMap, map, uniquePatterns, candidatePosition, sequeunce);
            (Pattern pattern, Vector2I patternPosition) = SelectPattern(tileMap, map, uniquePatterns, candidatePosition);

            if (pattern == null)
            {
                /* No valid pattern found at this position. Undo the last step and try a different step. */
                //UnapplyLastStep(tileMap, map, frontier, sequeunce, uniquePatterns, candidatePosition);
                //UnapplyLastStep(tileMap, map, null, null, uniquePatterns, candidatePosition);
                //continue;

                break;
            }

            SequenceStep sequenceStep = ApplyPatternAt(map, pattern, patternPosition);
            //sequeunce.Push(sequenceStep);

            /* Expand the frontier */
            //foreach (Vector2I collapsedCell in sequenceStep.Cells)
            //{
            //    foreach (Vector2I neighbor in tileMap.GetSurroundingCells(collapsedCell))
            //    {
            //        if (!map.TryGetValue(neighbor, out var value2) || value2.IsCollapsed /*|| value2.IsFrontier*/)
            //        {
            //            continue;
            //        }

            //        int chaos = GetChaosValue(map, tileMap, neighbor, uniquePatterns);
            //        //chaos = chaos == 0 ? -1 : chaos; // Enqueue with -1 to ensure these cells with no valid patterns get popped first.

            //        //frontier.Enqueue(neighbor, chaos);
            //        //map[neighbor].IsFrontier = true;
            //    }
            //}

            /* Rendering */
            foreach (var group in sequenceStep.Cells.GroupBy(c => map[c].Terrain))
            {
                tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I>(group.ToList()), 0, group.Key);
            }
        }

        GD.Print("ended");

        return targetCells; // TODO: Return a list of the unfilled cells
    }

    private (Vector2I, bool) GetLeastChaoticPosition(TileMap tileMap, Dictionary<Vector2I, MapCell> map, List<Pattern> uniquePatterns)
    {
        var leastChaoticPosition = new Vector2I();
        var chaos = int.MaxValue;
        var found = false;

        foreach (var kv in map)
        {
            if (kv.Value.IsCollapsed)
            {
                continue;
            }

            int newChaos = GetChaosValue(map, tileMap, kv.Key, uniquePatterns);
            if (newChaos < chaos)
            {
                leastChaoticPosition = kv.Key;
                chaos = newChaos;
                found = true;
            }
        }

        return (leastChaoticPosition, found);
    }

    /// <summary>
    /// Initialize a dictionary representing the state of the world.
    /// </summary>
    public Dictionary<Vector2I, MapCell> InitializeMap(TileMap tileMap, List<Vector2I> targetCells, List<Pattern> uniquePatterns)
    {
        var map = new Dictionary<Vector2I, MapCell>();

        foreach (Vector2I cell in targetCells)
        {
            map[cell] = new MapCell
            {
                Terrain = UNSET_TERRAIN,
                // Chaos = int.MaxValue
            };
        }

        Godot.Collections.Array<Vector2I> usedCells = tileMap.GetUsedCells(0);

        foreach (Vector2I cell in usedCells)
        {
            map[cell] = new MapCell
            {
                Terrain = tileMap.GetCellTileData(0, cell).Terrain
            };
        }

        foreach (Vector2I cell in usedCells)
        {
            //map[cell].Chaos = GetChaosValue(map, tileMap, cell, uniquePatterns);
        }

        return map;
    }

    /// <summary>
    /// Add intial positions to the frontier. The frontier exists for optimization purposes.
    /// </summary>
    public PriorityQueue<Vector2I, int> InitializeFrontier(TileMap tileMap, Dictionary<Vector2I, MapCell> map, List<Pattern> uniquePatterns)
    {
        var frontier = new PriorityQueue<Vector2I, int>();

        foreach (Vector2I cell in tileMap.GetUsedCells(0))
        {
            foreach (var neighbor in tileMap.GetSurroundingCells(cell))
            {
                if (map.TryGetValue(neighbor, out var value) && !value.IsCollapsed)
                {
                    frontier.Enqueue(neighbor, GetChaosValue(map, tileMap, neighbor, uniquePatterns));
                    //map[neighbor].IsFrontier = true;
                }
            }
        }

        return frontier;
    }

    /// <summary>
    /// Extract evey unique pattern from the sample. Depending on the PatternShape, it will include rotations.
    /// If the same pattern is encountered twice, the frequency is incremented.
    /// </summary>
    public List<Pattern> ExtractUniquePatterns()
    {
        var patterns = new List<Pattern>();
        var patternsTracker = new Dictionary<Pattern, Pattern>(); /* Using a Dictionary instead of HashMap becayse HashMaps suck. */

        foreach (Vector2I position in Sample.Keys)
        {
            Dictionary<Vector2I, PatternCell> unrotatedPattern = MapHelper.GetSubmap(Sample, PatternShape.Cells, position);
            var patternRotations = new List<Pattern>();

            if (unrotatedPattern == null)
            {
                continue;
            }

            IEnumerable<int> angles = unrotatedPattern.Any(kv => !kv.Value.IsRotatable)
                ? new int[] { 0 }
                : PatternShape.SuperimposedRotations;

            foreach (int angle in angles)
            {
                patternRotations.Add(new()
                {
                    Cells = PatternShape.RotatePattern(unrotatedPattern, angle)
                });
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
    public (Pattern, Vector2I) SelectPattern(TileMap tileMap, Dictionary<Vector2I, MapCell> map, List<Pattern> uniquePatterns, Vector2I candidatePosition/*, Stack<SequenceStep> sequence*/)
    {
        var validPatternsAndPositions = new List<(Pattern, Vector2I)>();

        foreach (Pattern pattern in uniquePatterns)
        {
            foreach (Vector2I patternCellPosition in pattern.Cells.Keys)
            {
                Vector2I patternPosition = candidatePosition - patternCellPosition;
                //bool wasPatternTriedPreviously =
                //    sequence.TryPeek(out var value)
                //    && value.TriedPatterns != null
                //    && value.TriedPatterns.Contains((pattern, patternPosition));

                if (/*!wasPatternTriedPreviously &&*/ CanApplyPatternAt(tileMap, map, pattern, patternPosition))
                {
                    validPatternsAndPositions.Add((pattern, patternPosition));
                }
            }
        }

        if (validPatternsAndPositions.Count == 0)
        {
            return (null, new(0,0));
        }

        /* Select a random pattern + position weighted by the pattern's frequency */

        int idx = RandomHelper.SelectRandomWeighted(validPatternsAndPositions, pat => pat.Item1.Frequency, Random);
        
        return validPatternsAndPositions[idx];
    }

    /// <summary>
    /// Can the pattern at the position be applied? If the pattern correctly overlaps the map (any overlap has the same cell type)
    /// and the pattern will not be placed adjacent to any existing cells then true.
    /// </summary>
    public bool CanApplyPatternAt(TileMap tileMap, Dictionary<Vector2I, MapCell> map, Pattern pattern, Vector2I patternPosition)
    {
        int overlaps = 0;
        int empty = 0;

        foreach (Vector2I cellPositionInPattern in pattern.Cells.Keys)
        {
            Vector2I cellPositionInMap = patternPosition + cellPositionInPattern;
            bool isInMap = map.TryGetValue(cellPositionInMap, out var mapCell);

            if (!isInMap)
            {
                continue;
            }

            if (mapCell.IsCollapsed)
            {
                if (mapCell.Terrain == pattern.Cells[cellPositionInPattern].Terrain)
                {
                    overlaps++;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                empty++;
            }

            /* Prevent uncloseable gaps. */
            if (!PatternShape.CanCloseGaps && WouldCreateGap(map, pattern, patternPosition))
            {
                return false;
            }

            /* Check if the pattern will be adjacently placed next to any existing cells. */
            /* If so, return false since placing adjacently could result in invalid cell adjacencies. */
            foreach (Vector2I neighborPosition in PatternShape.GetAdjacencies(cellPositionInMap))
            {
                bool isInPattern = pattern.Cells.ContainsKey(neighborPosition - patternPosition);

                if (isInPattern)
                {
                    continue;
                }

                if (map.TryGetValue(neighborPosition, out var value2) && value2.IsCollapsed && !mapCell.IsCollapsed)
                {
                    return false;
                }
            }
        }

        return overlaps > 0 && empty > 0;
    }

    internal bool WouldCreateGap(Dictionary<Vector2I, MapCell> map, Pattern pattern, Vector2I patternPosition)
    {
        foreach (var patternCellPosition in pattern.Cells.Keys)
        {
            Vector2I mapCellPosition = patternCellPosition + patternPosition;

            if (!map.TryGetValue(mapCellPosition, out var mapCell) || mapCell.IsCollapsed)
            {
                continue;
            }

            if (PartiallyCollapsedRows.ContainsKey(mapCellPosition.X) || PartiallyCollapsedColumns.ContainsKey(mapCellPosition.Y))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Apply the pattern at the position. Assuming it is a valid position for the pattern.
    /// </summary>
    /// <returns>
    /// A SequenceStep containing the position of the pattern and the cells that were applied.
    /// </returns>
    public SequenceStep ApplyPatternAt(Dictionary<Vector2I, MapCell> map, Pattern pattern, Vector2I patternPosition)
    {
        var appliedCells = new List<Vector2I>();
        var frontieredCells = new List<Vector2I>();

        foreach (Vector2I cellPositionInPattern in pattern.Cells.Keys)
        {
            var cellPositionInMap = cellPositionInPattern + patternPosition;

            if (!map.TryGetValue(cellPositionInMap, out var mapCell))
            {
                continue;
            }

            if (!mapCell.IsCollapsed)
            {
                //if (mapCell.IsFrontier)
                //{
                //    frontieredCells.Add(cellPositionInMap);
                //}

                map[cellPositionInMap].Terrain = pattern.Cells[cellPositionInPattern].Terrain;
                //map[cellPositionInMap].IsFrontier = false;
                appliedCells.Add(cellPositionInMap);
            }
        }

        return new SequenceStep
        {
            Cells = appliedCells,
            FrontieredCells = frontieredCells,
            Position = patternPosition,
            Pattern = pattern,
        };
    }

    /// <summary>
    /// Gets the "chaos" of the cell. Chaos is essentially a count of how many patterns are valid given this cell and the state of the world.
    /// </summary>
    public int GetChaosValue(Dictionary<Vector2I, MapCell> map, TileMap tileMap, Vector2I cellPosition, List<Pattern> uniquePatterns)
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
                    if (CanApplyPatternAt(tileMap, map, pattern, cellPosition + patternCellPosition))
                    {
                        chaos++;
                    }
                }
            }
        }

        return chaos;
    }

    public void UnapplyLastStep(TileMap tileMap, Dictionary<Vector2I, MapCell> map, PriorityQueue<Vector2I, int> frontier, Stack<SequenceStep> sequence,
        List<Pattern> uniquePatterns, Vector2I candidatePosition)
    {
        SequenceStep previousStep = sequence.Pop();

        foreach (var cell in previousStep.Cells)
        {
            map[cell].Terrain = UNSET_TERRAIN;
        }

        //foreach (var frontieredCell in previousStep.FrontieredCells)
        //{
        //    if (!map[frontieredCell].IsFrontier)
        //    {
        //        frontier.Enqueue(frontieredCell, GetChaosValue(map, tileMap, candidatePosition, uniquePatterns));
        //        map[frontieredCell].IsFrontier = true;
        //    }
        //}

        frontier.Enqueue(candidatePosition, GetChaosValue(map, tileMap, candidatePosition, uniquePatterns));
        //map[candidatePosition].IsFrontier = true;

        tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I>(previousStep.Cells), 0, UNSET_TERRAIN); // TODO: Make this depend on the render mode...

        SequenceStep previousStep2 = sequence.Peek(); // TODO: what to do if we pop the root sequeunce step?
        previousStep2.TriedPatterns ??= new List<(Pattern, Vector2I)>();
        previousStep2.TriedPatterns.Add((previousStep.Pattern, previousStep.Position));
    }
}