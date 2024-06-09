namespace ProjectMatchstick.Generation.Steps;

public struct TerrainRule
{
    public TerrainId NeighborTerrainId;
    public double Weight;

    public TerrainRule(TerrainId neighborTerrainId, double percentage)
    {
        NeighborTerrainId = neighborTerrainId;
        Weight = percentage;
    }
}