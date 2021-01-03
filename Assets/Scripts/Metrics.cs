using UnityEngine;

public enum Direction
{
    W, N, E, S
}

public static class DirectionExtensions
{
    public static Direction Opposite(this Direction direction)
    {
        return (Direction)(((int)direction + 2) % 4);
    }
}

public enum HexEdgeType
{
    Flat, Slope, Cliff
}

public static class Metrics
{
    public const int chunkSizeX = 5, chunkSizeZ = 5;

    public const float radius = 10f;

    public static Vector3[] corners = {
        new Vector3(-radius, 0f, -radius),
        new Vector3(-radius, 0f, radius),
        new Vector3(radius, 0f, radius),
        new Vector3(radius, 0f, -radius)
    };

    public const float solidFactor = 0.75f;

    public const float elevationStep = radius * 0.6f;
    public const float waterElevationOffset = -radius * 0.2f;

    public const int terracesPerSlope = 2;
    public const int terracesSteps = terracesPerSlope * 2 + 1;
    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float h = 1f / terracesSteps * step;
        float v = ((step + 1) / 2) * (float)(1f / (terracesPerSlope + 1));
        Vector3 c = new Vector3(
            a.x + (b.x - a.x) * h, 
            a.y + (b.y - a.y) * v,
            a.z + (b.z - a.z) * h);
        return c;
    }
    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = 1f / terracesSteps * step;
        return Color.Lerp(a, b, h);
    }
    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        if (elevation1 == elevation2) {
            return HexEdgeType.Flat;
        }
        int delta = elevation2 - elevation1;
        if (delta == 1 || delta == -1) {
            return HexEdgeType.Slope;
        }
        return HexEdgeType.Cliff;
    }

    public static Texture2D noiseSource;
    public const float noiseScale = 0.003f;
    public static Vector4 SampleNoise(Vector3 position)
    {
        return noiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
    }
    public const float cellPerturbStrength = radius * 0.5f;
    public const float elevationPerturbStrength = radius * 0.15f;
    public static Vector3 Perturb(Vector3 position)
    {
        Vector4 sample = SampleNoise(position);
        position.x += (sample.x * 2f - 1f) * cellPerturbStrength;
        position.z += (sample.z * 2f - 1f) * cellPerturbStrength;
        return position;
    }
    public static float GetRand(Vector3 position)
    {
        Vector4 sample = SampleNoise(position);
        return sample.x;
    }
}
