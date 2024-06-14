using Godot;

namespace ProjectMatchstick.Services.Generation.Steps;

public interface IGenerationStep
{
    /// <summary>
    /// Generate using a list of cells to fill.
    /// </summary>
    /// <param name="tileMap">The Godot TileMap.</param>
    /// <param name="targetCells">A list of cells which will be filled by the step.</param>
    /// <param name="generatedTerrain">A dictionary of the already generated terrains.</param>
    /// <param name="mode">The generation render mode.</param>
    /// <returns>A list of "skipped" cells.</returns>
    List<Vector2I> Generate(TileMap tileMap, List<Vector2I> targetCells, GenerationRenderMode mode);
}
