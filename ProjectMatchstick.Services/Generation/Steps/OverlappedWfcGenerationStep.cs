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
        public int[,] Modules { get; set; }
        public int Frequency { get; set; }
        public int HashValue { get; set; }

        public int Size { get => Modules.GetLength(0); }

        public override int GetHashCode()
        {
            return HashValue;
        }
        public override bool Equals(object obj)
        {
            if (obj is Pattern other)
            {
                if (Modules.Length != other.Modules.Length || Modules.GetLength(0) != Modules.GetLength(0))
                {
                    return false;
                }

                for (int i = 0; i < Modules.GetLength(0); i++)
                {
                    for (int j = 0; j < Modules.GetLength(1); j++)
                    {
                        if (Modules[i, j] != other.Modules[i, j])
                        {
                            return false;
                        }
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

    public List<Vector2I> Generate(TileMap tileMap, List<Vector2I> targetCells, GenerationRenderMode mode) { throw new NotImplementedException(); }
    //{
    //    var map = new Dictionary<Vector2I, Cell>();
    //    var frontier = new PriorityQueue<Vector2I, int>();
    //    var uniquePatterns = ExtractUniquePatterns();
    //    var sequeunce = new Stack<SequenceStep>();

    //    while (frontier.Count > 0)
    //    {
    //        var leastChaoticCell = frontier.Dequeue();

    //        var pat = SelectPattern(map, frontier, uniquePatterns, leastChaoticCell);
    //        var sequenceStep = ApplyPattern(map, pat);
    //        sequeunce.Push(sequenceStep);

    //        foreach (var neighbor in tileMap.GetSurroundingCells(leastChaoticCell)
    //            .Where(n => map.ContainsKey(n))
    //        {

    //        }
    //    }
    //}

    public List<Pattern> ExtractUniquePatterns()
    {
        var patterns = new List<Pattern>();
        var patternsTracker = new HashSet<Pattern>();

        for (var x = 0; x <= Sample.GetLength(0) - PatternSize; x++)
        {
            for (var y = 0; y <= Sample.GetLength(1) - PatternSize; y++)
            {
                var unrotatedPattern = MatrixHelper.GetSubmatrix(Sample, x, y, PatternSize);

                var patternRotations = new List<Pattern>
                {
                    new() { Modules = unrotatedPattern },
                    new() { Modules = MatrixHelper.RotateClockwise(MatrixHelper.Clone(unrotatedPattern), 1) },
                    new() { Modules = MatrixHelper.RotateClockwise(MatrixHelper.Clone(unrotatedPattern), 2) },
                    new() { Modules = MatrixHelper.RotateClockwise(MatrixHelper.Clone(unrotatedPattern), 3) },
                };

                foreach (var pat in patternRotations)
                {
                    var isFound = patternsTracker.TryGetValue(pat, out var foundPattern);

                    if (isFound)
                    {
                        foundPattern.Frequency++;
                    }
                    else
                    {
                        pat.Frequency = 1;

                        patternsTracker.Add(pat);
                        patterns.Add(pat);
                    }
                }
            }
        }

        return patterns;
    }

    public (Pattern, Vector2I) SelectPattern(Dictionary<Vector2I, Cell> map, HashSet<Vector2I> targetCellsSet, PriorityQueue<Vector2I, int> frontier, List<Pattern> uniquePatterns, Vector2I leastChaoticCell)
    {
        var validPatterns = new List<(Pattern, Vector2I)>();

        foreach (var pat in uniquePatterns)
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (CanApply(map, targetCellsSet, pat, x, y))
                    {
                        validPatterns.Add((pat, new Vector2I(x, y)));
                    }
                }
            }
        }

        var idx = RandomHelper.SelectRandomWeighted(validPatterns, pat => pat.Item1.Frequency, new Random());

        return validPatterns[idx];
    }

    public bool CanApply(Dictionary<Vector2I, Cell> map, HashSet<Vector2I> targetCellsSet, Pattern pattern, int x, int y)
    {
        for (int i = x; i < x + pattern.Size; i++)
        {
            for (int j = y; j < y + pattern.Size; j++)
            {
                var vec = new Vector2I(i, j);

                if (!targetCellsSet.Contains(vec))
                {
                    // Pattern is partly "out of bounds"
                    return false;
                }

                var isCellOccupied = map.TryGetValue(vec, out var cell);

                if (isCellOccupied && cell.Terrain != pattern.Modules[i, j])
                {
                    return false;
                }
            }
        }

        return true;
    }
}
