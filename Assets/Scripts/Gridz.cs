using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Gridz : MonoBehaviour
{
    //int width = 20;
    //int height = 20;
    int chunkCountX, chunkCountZ;
    int cellCountX, cellCountZ;

    public Cell cellPrefab;
    public Text cellLabelPrefab;
    public GridChunk chunkPrefab;

    public Texture2D noiseSource;

    Cell[] cells;
    GridChunk[] chunks;

    private void Awake()
    {
        Metrics.noiseSource = noiseSource;
    }

    private void OnEnable()
    {
        if (!Metrics.noiseSource) {
            Metrics.noiseSource = noiseSource;
        }
    }

    public bool CreateMap(int xx, int zz)
    {
        if (xx <= 0 || xx % Metrics.chunkSizeX != 0 ||
            zz <= 0 || zz % Metrics.chunkSizeZ != 0) {
            Debug.LogError("Unsupported map size " + xx + zz);
            return false;
        }

        cellCountX = xx;
        cellCountZ = zz;
        chunkCountX = xx / Metrics.chunkSizeX;
        chunkCountZ = zz / Metrics.chunkSizeZ;

        chunks = new GridChunk[chunkCountZ * chunkCountX];
        for (int z = 0, i = 0; z < chunkCountZ; z++) {
            for (int x = 0; x < chunkCountX; x++) {
                GridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }

        cells = new Cell[cellCountZ * cellCountX];
        for (int z = 0, i = 0; z < cellCountZ; z++) {
            for (int x = 0; x < cellCountX; x++) {
                CreateCell(x, z, i++);
            }
        }

        return true;
    }

    void CreateCell(int x, int z, int i)
    {
        Vector3 position = new Vector3(x * Metrics.radius * 2f, 0f, z * Metrics.radius * 2f);
        Cell cell = Instantiate(cellPrefab);
        // cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = Coordinates.FromOffsetCoordinates(x, z);

        if (x > 0) {
            cell.SetNeighbor(Direction.W, cells[i - 1]);
        }
        if (z > 0) {
            cell.SetNeighbor(Direction.S, cells[i - cellCountX]);
        }

        cells[i] = cell;

        Text label = Instantiate<Text>(cellLabelPrefab);
        // label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToString();
        cell.uiRect = label.rectTransform;

        // add cell to chunk
        int chunkX = x / Metrics.chunkSizeX;
        int chunkZ = z / Metrics.chunkSizeZ;
        GridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];
        int localX = x - chunkX * Metrics.chunkSizeX;
        int localZ = z - chunkZ * Metrics.chunkSizeZ;
        chunk.AddCell(localX, localZ, cell);

        cell.Elevation = 0; // Refresh
    }

    public Cell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        Coordinates coordinates = Coordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * cellCountX;
        return cells[index];
    }

    public Cell GetCell(int xOffset, int zOffset)
    {
        return cells[xOffset + zOffset * cellCountX];
    }

    public Cell GetCell(int cellIndex)
    {
        return cells[cellIndex];
    }

    public void FindDistancesTo(Cell cell)
    {
        for (int i = 0; i < cells.Length; i++) {
            cells[i].Distance = cell.coordinates.DistanceTo(cells[i].coordinates);
        }
    }

    public void Save(BinaryWriter writer)
    {
        for (int i = 0; i < cells.Length; i++) {
            cells[i].Save(writer);
        }
    }

    public void Load(BinaryReader reader)
    {
        for (int i = 0; i < cells.Length; i++) {
            cells[i].Load(reader);
        }
        for (int i = 0; i < chunks.Length; i++) {
            chunks[i].Refresh();
        }
    }

}
