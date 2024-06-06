namespace ProjectMatchstick.Generation;

using Godot;
using ProjectMatchstick.Generation.Strategies;
using System.Collections.Generic;
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
            GD.Print("Generating...");

            var tiles = new List<Vector2I>();

            for (int i = MinX; i <= MaxX; i++)
            {
                for (int j = MinY; j <= MaxY; j++)
                {
                    tiles.Add(new Vector2I(i, j));
                }
            }

            var gen0 = new UniformGenerationStep();
            gen0.Generate(TileMap, tiles, GenerationRenderMode.ON_STEP_COMPLETE);

            var gen2 = new WfcGenerationStep(null, Seed, TerrainId.VOID);
            gen2.Generate(TileMap, tiles, GenerationRenderMode.ON_STEP_COMPLETE);

            GD.Print("Finished Generating");
        });
    }
}
