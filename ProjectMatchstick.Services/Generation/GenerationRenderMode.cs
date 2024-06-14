namespace ProjectMatchstick.Services.Generation;

/// <summary>
/// IMMEDIATE: Render as the tiles are decided by the algorithm. Least performant and produces visual bugs. Good for debugging and level design.
/// ON_STEP_COMPLETE: Render tiles when the generation step completes. Balance.
/// </summary>
public enum GenerationRenderMode
{
    IMMEDIATE,
    ON_STEP_COMPLETE
}
