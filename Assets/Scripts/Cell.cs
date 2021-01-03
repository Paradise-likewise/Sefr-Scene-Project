using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Cell : MonoBehaviour
{
    public GridChunk chunk;
    public Coordinates coordinates;
    public RectTransform uiRect;

    int terrainTypeIndex;
    public int TerrainTypeIndex {
        get {
            return terrainTypeIndex;
        }
        set {
            if (terrainTypeIndex == value) return;
            terrainTypeIndex = value;
            Refresh(false);
        }
    }

    int elevation = int.MinValue;
    public int Elevation {
        get {
            return elevation;
        }
        set {
            if (elevation == value) return;
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * Metrics.elevationStep;
            position.y += (Metrics.SampleNoise(position).y * 2f - 1f) * Metrics.elevationPerturbStrength;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            Refresh(false);
        }
    }

    public Vector3 Position {
        get { return transform.localPosition; }
    }

    int waterLevel;
    public int WaterLevel {
        get { return waterLevel; }
        set {
            if (waterLevel == value) return;
            waterLevel = value;
            Refresh(false);
        }
    }
    public bool IsUnderwater {
        get { return elevation < waterLevel; }
    }

    public float WaterSurfaceY {
        get {
            return waterLevel * Metrics.elevationStep + Metrics.waterElevationOffset;
        }
    }

    int featureTypeIndex;
    public int FeatureTypeIndex {
        get { return featureTypeIndex; }
        set {
            if (featureTypeIndex == value) return;
            featureTypeIndex = value;
            Refresh(true);
        }
    }

    [SerializeField]
    Cell[] neighbors;

    public Cell GetNeighbor(Direction direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(Direction direction, Cell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(Direction direction)
    {
        return Metrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    int distance;
    public int Distance {
        get { return distance; }
        set {
            distance = value;
        }
    }

    public int SearchHeuristic { get; set; }
    public int SearchPriority {
        get { return distance + SearchHeuristic; }
    }
    public Cell NextWithSamePriority { get; set; }

    public int SearchPhase { get; set; }

    void Refresh(bool selfOnly)
    {
        if (chunk) {
            chunk.Refresh();
            if (!selfOnly) {
                int prev_i = 3;
                for (int i = 0; i < neighbors.Length; i++) {
                    Cell neighbor = neighbors[i];
                    if (neighbor != null && neighbor.chunk != chunk) {
                        neighbor.chunk.Refresh();

                        Cell corner_cell = neighbor.GetNeighbor((Direction)prev_i);
                        if (corner_cell != null && corner_cell.chunk != chunk) {
                            corner_cell.chunk.Refresh();
                        }
                    }
                    prev_i = i;
                }
            }
        }
    }

    void RefreshPosition()
    {
        Vector3 position = transform.localPosition;
        position.y = elevation * Metrics.elevationStep;
        transform.localPosition = position;

        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = elevation * -Metrics.elevationStep;
        uiRect.localPosition = uiPosition;
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)terrainTypeIndex);
        writer.Write((byte)(elevation + 127));
        writer.Write((byte)(waterLevel + 127));
        writer.Write((byte)featureTypeIndex);
    }

    public void Load(BinaryReader reader)
    {
        terrainTypeIndex = reader.ReadByte();
        elevation = reader.ReadByte() - 127;
        RefreshPosition();
        waterLevel = reader.ReadByte() - 127;
        featureTypeIndex = reader.ReadByte();
    }
}
