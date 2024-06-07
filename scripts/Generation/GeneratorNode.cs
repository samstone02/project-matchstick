namespace ProjectMatchstick.Generation;

using Godot;
using ProjectMatchstick.Generation.Strategies;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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

            var gen0 = new UniformGenerationStep();
            gen0.Generate(TileMap, tiles, GenerationRenderMode.IMMEDIATE);

            TileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { new(0, 0) }, 0, (int)TerrainId.WALL);

            var gen2 = new WfcGenerationStep(null, Seed, TerrainId.VOID, new HashSet<TerrainId>{ TerrainId.VOID });
            gen2.Generate(TileMap, tiles, GenerationRenderMode.IMMEDIATE);

            sw.Stop();
            GD.Print("Finished Generating");

            GD.Print("Total Time for IMMEDIATE: " + sw.Elapsed.TotalSeconds);
        });
    }
}
