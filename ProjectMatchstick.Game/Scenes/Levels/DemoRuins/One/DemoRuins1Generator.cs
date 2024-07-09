using Godot;
using ProjectMatchstick.Services.Generation;
using ProjectMatchstick.Services.Generation.PatternShapes;
using ProjectMatchstick.Services.Generation.Steps;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ProjectMatchstick.Game.Scenes.Levels.DemoRuins.One;

public partial class DemoRuins1Generator : Node
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
    PackedScene SampleScene { get; set; }

    [Export]
    TileMap Map { get; set; }

    public override void _Ready()
    {
        var thread = new Thread(() =>
        {
            GD.Print("Finished!");

            var tiles = new List<Vector2I>();

            for (int i = MinX; i <= MaxX; i++)
            {
                for (int j = MinY; j <= MaxY; j++)
                {
                    tiles.Add(new Vector2I(i, j));
                }
            }

            var sample = (TileMap)SampleScene.Instantiate().FindChild("DemoRuinsHexTileMap");

            var overlappedWfc = new OverlappedWfcGenerationStep
            {
                Sample = sample.GetUsedCells(0).ToDictionary(vec => vec, vec => sample.GetCellTileData(0, vec).Terrain),
                PatternSize = 2,
                //PatternShape = new HexagonPatternShape(2),
                PatternShape = new SqaurePatternShape(4),
                Random = new System.Random(Seed),
            };

            overlappedWfc.Generate(Map, tiles, GenerationRenderMode.IMMEDIATE);

            GD.Print("Finished!");
        });

        thread.Start();
    }
}
