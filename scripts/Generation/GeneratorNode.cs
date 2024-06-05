namespace ProjectMatchstick.Generation;

using Godot;
using ProjectMatchstick.Generation.Strategies;
using System.Threading.Tasks;

public partial class GeneratorNode : Node
{
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

            var tc = new Vector2I(0, 0);
            var bc = new Vector2I(MaxX, MaxY);

            var gen0 = new UniformGenerationStep();
            gen0.Generate(TileMap, tc, bc);

            var gen2 = new WfcGenerationStep(null, Seed, TerrainId.VOID);
            gen2.Generate(TileMap, tc, bc);

            GD.Print("Finished Generating");
        });
    }
}
