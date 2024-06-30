using Godot;
using ProjectMatchstick.Services.Generation;
using ProjectMatchstick.Services.Generation.Steps;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProjectMatchstick.Scenes;

public partial class GeneratorNode : Node
{
    [Export]
    public int MinX { get; set; }

    [Export]
    public int MinY { get; set; }

    [Export]
    public int MaxX { get; set; }

    [Export]
    public int MaxY { get; set; }

    [Export]
    public int Seed { get; set; }

    [Export]
    TileMap TileMap { get; set; }

    public override void _Ready()
    {
        GD.Print("Hello!");
        //Task.Run(() =>
        //{
            Stopwatch sw = Stopwatch.StartNew();

            GD.Print("Generating...");
            sw.Start();

            var tiles = new List<Vector2I>();

            for (int i = MinX; i <= MaxX; i++)
            {
                for (int j = MinY; j <= MaxY; j++)
                {
                    tiles.Add(new Vector2I(i, j));
                }
            }

            //var gen0 = new UniformGenerationStep();
            //gen0.Generate(TileMap, tiles, GenerationRenderMode.IMMEDIATE);

            TileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I>
            {
                new(1, 0),
                //new(0, 1),
                //new(1, 1),
                //new(-1, 0),
                //new(0, -1),
                //new(-1, -1),
            },0, 1);

            //var x = TileMap.GetSurroundingCells(new(0, 0));

            //var gen2 = new SimpleTiledWfcGenerationStep(
            //    new Dictionary<TerrainId, List<TerrainRule>>
            //    {
            //        { TerrainId.VOID, new List<TerrainRule> { new(TerrainId.VOID, 20), new(TerrainId.WATER, 0.3), new(TerrainId.LAND, 0.3), new(TerrainId.WALL, 0.3) } },
            //        { TerrainId.WATER, new List<TerrainRule> { new(TerrainId.VOID, 10), new(TerrainId.WATER, 2.0), new(TerrainId.LAND, 0.25) } },
            //        { TerrainId.LAND, new List<TerrainRule> { new(TerrainId.VOID, 7.0), new(TerrainId.WATER, 2.0), new(TerrainId.LAND, 6.0), new(TerrainId.WALL, 1.0) } },
            //        { TerrainId.WALL, new List<TerrainRule> { new(TerrainId.LAND, 0.5), new(TerrainId.WALL, 3.0) } }
            //    }, Seed, TerrainId.VOID, new HashSet<TerrainId> { TerrainId.VOID });
            //tiles = gen2.Generate(TileMap, tiles, GenerationRenderMode.IMMEDIATE);

            //var gen3 = new SimpleTiledWfcGenerationStep(
            //    new Dictionary<TerrainId, List<TerrainRule>>
            //    {
            //        { TerrainId.VOID, new List<TerrainRule> { new(TerrainId.WATER, 1) } },
            //        { TerrainId.WATER, new List<TerrainRule> { new(TerrainId.WATER, 2.0) } },
            //        { TerrainId.LAND, new List<TerrainRule> { new(TerrainId.WATER, 1) } },
            //        { TerrainId.WALL, new List<TerrainRule> { new(TerrainId.WATER, 1.0) } }
            //    }, Seed, TerrainId.VOID, new HashSet<TerrainId> { TerrainId.VOID });
            //gen3.Generate(TileMap, tiles, GenerationRenderMode.IMMEDIATE);

            var gen3 = new OverlappedWfcGenerationStep
            {
                PatternSize = 2,
                Sample = new int[,]
                {
                    { 2, 2, 2, 2, 2, 1, 1, },
                    { 2, 3, 3, 2, 2, 1, 1, },
                    { 2, 3, 3, 2, 2, 1, 1, },
                    { 2, 2, 2, 3, 2, 1, 1, },
                    { 2, 2, 2, 2, 2, 1, 1, },
                    { 1, 1, 1, 1, 1, 2, 1, },
                    { 1, 1, 1, 1, 1, 1, 1, },
                }
            };
            gen3.Generate(TileMap, tiles, 0);

            sw.Stop();
            GD.Print("Finished Generating");

            var emptyTiles = new List<Vector2I>();

            foreach (var tile in tiles)
            {
                if (TileMap.GetCellTileData(0, tile) == null)
                {
                    emptyTiles.Add(tile);
                }
            }

            GD.Print("Total Time for IMMEDIATE: " + sw.Elapsed.TotalSeconds);

            GD.Print("FINISHED!");
        //});
    }
}
