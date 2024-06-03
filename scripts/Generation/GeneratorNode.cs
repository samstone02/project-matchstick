namespace Matchstick.Generation;

using Godot;
using Matchstick.Generation.Steps;
using ProjectMatchstick.Generation.Strategies;
using ProjectMatchstick.scripts.Generation;
using ProjectMatchstick.scripts.Generation.Strategies;
using System.Threading.Tasks;

public partial class GeneratorNode : Node
{
    [Export]
    public int MaxX { get; set; } = 40;

    [Export]
    public int MaxY { get; set; } = 40;

    [Export]
    public int Seed { get; set; } = -1;

    [Export]
    TileMap TileMap { get; set; }

    public override void _Ready()
    {
        Task.Run(() =>
        {
            GD.Print("Generating...");

            var tc = new Vector2I(-20, -20);
            var bc = new Vector2I(20, 20);

            var gen0 = new UniformGenerationStep();
            gen0.TerrainId = TerrainIds.VOID; // 4 = void rn
            gen0.Generate(TileMap, tc, bc);

            var gen2 = new WfcGenerationStep(null, Seed);
            gen2.Generate(TileMap, tc, bc);

            GD.Print("Finished Generating");
        });
    }
}
