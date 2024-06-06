using Godot;
using ProjectMatchstick.Generation.Shapes;
using System.Collections.Generic;

namespace ProjectMatchstick.Generation.Steps;

public interface IGenerationStep
{
    /// <summary>
    /// Generate using a shape.
    /// </summary>
    /// <param name="tileMap">The Godot TileMap.</param>
    /// <param name="cellsToFill">A list of cells which will be filled by the step.</param>
    /// <param name="generatedTerrain">A dictionary of the already generated terrains.</param>
    /// <param name="mode">The generation render mode.</param>
    void Generate(TileMap tileMap, IShape generationShape, GenerationRenderMode mode);

    /// <summary>
    /// Generate using a list of cells to fill.
    /// </summary>
    /// <param name="tileMap">The Godot TileMap.</param>
    /// <param name="cellsToFill">A list of cells which will be filled by the step.</param>
    /// <param name="generatedTerrain">A dictionary of the already generated terrains.</param>
    /// <param name="mode">The generation render mode.</param>
    void Generate(TileMap tileMap, List<Vector2I> cellsToFill, GenerationRenderMode mode);
}
