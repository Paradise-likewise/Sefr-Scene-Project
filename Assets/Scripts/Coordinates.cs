using UnityEngine;

[System.Serializable]
public struct Coordinates
{
    [SerializeField]
    private int x, z;
    public int X { get { return x; } }
    public int Z { get { return z; } }
    public Coordinates(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    public static Coordinates FromOffsetCoordinates(int x, int z)
    {
        return new Coordinates(x, z);
    }
    public static Coordinates FromPosition(Vector3 position)
    {
        float x = position.x / (Metrics.radius * 2f);
        float z = position.z / (Metrics.radius * 2f);
        int iX = Mathf.RoundToInt(x);
        int iZ = Mathf.RoundToInt(z);
        return new Coordinates(iX, iZ);
    }

    public int DistanceTo(Coordinates other)
    {
        return
            (x < other.x ? other.x - x : x - other.x) +
            (z < other.z ? other.z - z : z - other.z);
    }
    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Z.ToString() + ")";
    }
    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Z.ToString();
    }
}
