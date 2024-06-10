using Godot;
using ProjectMatchstick.Generation.Steps;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProjectMatchstick.Generation;

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
        Task.Run(() =>
        {
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

            TileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { new(50, 50) }, 0, (int)TerrainId.WALL);

            var gen2 = new WfcGenerationStep(
                new Dictionary<TerrainId, List<TerrainRule>>
                {
                    { TerrainId.VOID, new List<TerrainRule> { new(TerrainId.VOID, 20), new(TerrainId.WATER, 0.3), new(TerrainId.LAND, 0.3), new(TerrainId.WALL, 0.3) } },
                    { TerrainId.WATER, new List<TerrainRule> { new(TerrainId.VOID, 10), new(TerrainId.WATER, 2.0), new(TerrainId.LAND, 0.25) } },
                    { TerrainId.LAND, new List<TerrainRule> { new(TerrainId.VOID, 7.0), new(TerrainId.WATER, 2.0), new(TerrainId.LAND, 6.0), new(TerrainId.WALL, 1.0) } },
                    { TerrainId.WALL, new List<TerrainRule> { new(TerrainId.LAND, 0.5), new(TerrainId.WALL, 2.0) } }
                }, Seed, TerrainId.VOID, new HashSet<TerrainId>{ TerrainId.VOID });
            tiles = gen2.Generate(TileMap, tiles, GenerationRenderMode.IMMEDIATE);

            var gen3 = new WfcGenerationStep(
                new Dictionary<TerrainId, List<TerrainRule>>
                {
                    { TerrainId.VOID, new List<TerrainRule> { new(TerrainId.WATER, 1) } },
                    { TerrainId.WATER, new List<TerrainRule> { new(TerrainId.WATER, 2.0) } },
                    { TerrainId.LAND, new List<TerrainRule> { new(TerrainId.WATER, 1) } },
                    { TerrainId.WALL, new List<TerrainRule> { new(TerrainId.WATER, 1.0) } }
                }, Seed, TerrainId.VOID, new HashSet<TerrainId> { TerrainId.VOID });
            gen3.Generate(TileMap, tiles, GenerationRenderMode.IMMEDIATE);

            sw.Stop();
            GD.Print("Finished Generating");

            GD.Print("Total Time for IMMEDIATE: " + sw.Elapsed.TotalSeconds);

            for (int i = MinX; i <= MaxX; i++)
            {
                for (int j = MinY; j <= MaxY; j++)
                {
                    var sid = TileMap.GetCellTileData(0, new(i, j)).Terrain;
                }
            }

            GD.Print("FINISHED!");
        });
    }
}
