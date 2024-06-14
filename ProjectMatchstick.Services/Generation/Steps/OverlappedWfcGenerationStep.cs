using Godot;

namespace ProjectMatchstick.Services.Generation.Steps;

public class OverlappedWfcGenerationStep : IGenerationStep
{
    public class Pattern
    {
        public int[,] Modules { get; set; }
        public int Frequency { get; set; }
    }

    public int[,] Sample {  get; set; }

    public List<Vector2I> Generate(TileMap tileMap, List<Vector2I> targetCells, GenerationRenderMode mode)
    {
        //var patterns = ExtractUniquePatterns(10);
        //var rulset = GenerateRuleset(patterns);
        //Collapse(tileSet, ruleset);
        throw new NotImplementedException();
    }

    //private List<Pattern> ExtractUniquePatterns(int n)
    //{
    //    var patterns = new List<Pattern>();

    //    for (var x = 0; x < Sample.GetLength(0); x++)
    //    {
    //        for (var y = 0; y < Sample.GetLength(1); y++)
    //        {
    //            foreach (var pattern in GetPatterns(x, y, n))
    //            {
    //                pattern.
    //            }
    //        }
    //    }
    //}

    //private List<Pattern> GetPatterns(int x, int y, int n)
    //{
    //    return new List<Pattern>
    //    {
    //        new Pattern
    //        {
    //            GetSubarray(x, y, n, "zero")
    //        }
    //    };
    //}
}
