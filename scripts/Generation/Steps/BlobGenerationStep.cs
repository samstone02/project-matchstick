using Godot;
using ProjectMatchstick.Generation.Steps;

namespace ProjectMatchstick.Generation.Strategies;

public class BlobGenerationStep : IGenerationStep
{
    private int BlobRadius { get; }

    public BlobGenerationStep(int mountainRadius)
    {
        BlobRadius = mountainRadius;
    }

    public void Generate(TileMap tileMap, Vector2I topCorner, Vector2I bottomCorner)
    {
        var center = new Vector2I((topCorner.X + bottomCorner.X) / 2, (topCorner.Y + bottomCorner.Y) / 2);

        for (int i = center.X - BlobRadius; i <= center.X + BlobRadius; i++)
        {
            for (int j = center.Y - BlobRadius; j <= center.Y + BlobRadius; j++)
            {
                tileMap.SetCellsTerrainConnect(0, new Godot.Collections.Array<Vector2I> { new(i, j) }, 0, 2);
            }
        }
    }
}
