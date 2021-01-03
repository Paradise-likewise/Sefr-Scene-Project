using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Gridz grid;

    public int mapSizeX = 100, mapSizeZ = 100;
    int cellCount;


    CellPriorityQueue searchFrontier;
    int searchFrontierPhase = 0;

    public bool useFixedSeed;
    public int seed;

    [Range(0f, 0.5f)]
    public float jitterProbability = 0.25f;

    [Range(20, 200)]
    public int chunkSizeMin = 30;

    [Range(20, 200)]
    public int chunkSizeMax = 100;

    [Range(5, 95)]
    public int landPercentage = 50;

    [Range(1, 5)]
    public int waterLevel = 3;

    [Range(0f, 1f)]
    public float highRiseProbability = 0.25f;

    [Range(0f, 0.4f)]
    public float sinkProbability = 0.2f;

    [Range(-4, 0)]
    public int elevationMinimum = -2;

    [Range(6, 16)]
    public int elevationMaximum = 16;

    public int[] terrainMap;

    [Range(0.6f, 1f)]
    public float changeTerrainProbability = 0.8f;

    [Range(0.1f, 0.6f)]
    public float growFeatureProbability = 0.1f;


    Cell birthCell;
    List<Cell> cellList = new List<Cell>();

    private void Start()
    {
        GenerateMap(mapSizeX, mapSizeZ);
    }

    public void GenerateMap(int x, int z)
    {
        Random.State originalRandomState = Random.state;
        if (!useFixedSeed) {
            seed = Random.Range(0, int.MaxValue);
            seed ^= (int)System.DateTime.Now.Ticks;
            seed ^= (int)Time.unscaledTime;
            seed &= int.MaxValue;
        }
        Random.InitState(seed);
        Debug.Log("Initiate with seed " + seed.ToString());
        cellCount = x * z;
        grid.CreateMap(x, z);

        if (searchFrontier == null) {
            searchFrontier = new CellPriorityQueue();
        }
        for (int i = 0; i < cellCount; i++) {
            grid.GetCell(i).WaterLevel = waterLevel;
        }
        CreateLand();
        // SetTerrainType();

        for (int i = 0; i < cellCount; i++) {
            grid.GetCell(i).SearchPhase = 0;
        }
        Random.state = originalRandomState;
    }

    void CreateLand()
    {
        int landBudget = Mathf.RoundToInt(cellCount * landPercentage * 0.01f);
        while (landBudget > 0) {
            int chunkSize = Random.Range(chunkSizeMin, chunkSizeMax + 1);
            if (Random.value < sinkProbability) {
                landBudget = SinkTerrain(chunkSize, landBudget);
            }
            else {
                landBudget = RaiseTerrain(chunkSize, landBudget);
            }
        }
        CreateSpecialCell();
    }

    int RaiseTerrain(int chunkSize, int budget)
    {
        searchFrontierPhase += 1;
        Cell firstCell = GetRandomCell();
        firstCell.SearchPhase = searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        searchFrontier.Enqueue(firstCell);
        Coordinates centerCoord = firstCell.coordinates;

        int rise = Random.value < highRiseProbability ? 2 : 1;
        int size = 0;
        bool changeTerrain = Random.value < changeTerrainProbability;
        bool growFeature = Random.value < growFeatureProbability;
        while (size < chunkSize && searchFrontier.Count > 0) {
            Cell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = originalElevation + rise;
            if (newElevation > elevationMaximum) {
                continue;
            }
            current.Elevation = newElevation;
            SetCellProperty(current, changeTerrain, growFeature);

            if (originalElevation < waterLevel &&
                newElevation >= waterLevel && --budget == 0) {
                break;
            }
            size += 1;

            for (Direction d = Direction.W; d <= Direction.S; d++) {
                Cell neighbor = current.GetNeighbor(d);
                if (neighbor && neighbor.SearchPhase < searchFrontierPhase) {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = neighbor.coordinates.DistanceTo(centerCoord);
                    neighbor.SearchHeuristic = Random.value < jitterProbability ? 1 : 0;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }
        searchFrontier.Clear();
        return budget;
    }

    int SinkTerrain(int chunkSize, int budget)
    {
        searchFrontierPhase += 1;
        Cell firstCell = GetRandomCell();
        firstCell.SearchPhase = searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        searchFrontier.Enqueue(firstCell);
        Coordinates centerCoord = firstCell.coordinates;

        int sink = Random.value < highRiseProbability ? 2 : 1;
        int size = 0;
        bool changeTerrain = Random.value < changeTerrainProbability;
        bool growFeature = Random.value < growFeatureProbability;
        while (size < chunkSize && searchFrontier.Count > 0) {
            Cell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = current.Elevation - sink;
            if (newElevation < elevationMinimum) {
                continue;
            }
            current.Elevation = newElevation;
            SetCellProperty(current, changeTerrain, growFeature);

            if (originalElevation >= waterLevel &&
                newElevation < waterLevel) {
                budget += 1;
            }
            size += 1;

            for (Direction d = Direction.W; d <= Direction.S; d++) {
                Cell neighbor = current.GetNeighbor(d);
                if (neighbor && neighbor.SearchPhase < searchFrontierPhase) {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = neighbor.coordinates.DistanceTo(centerCoord);
                    neighbor.SearchHeuristic = Random.value < jitterProbability ? 1 : 0;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }
        searchFrontier.Clear();
        return budget;
    }

    /*
    void SetTerrainType()
    {
        for (int i = 0; i < cellCount; i++) {
            Cell cell = grid.GetCell(i);
            cell.TerrainTypeIndex = terrainMap[cell.Elevation - elevationMinimum];
        }
    }*/

    void SetCellProperty(Cell cell, bool changeTerrain, bool growFeature)
    {
        if (changeTerrain) {
            cell.TerrainTypeIndex = terrainMap[cell.Elevation - elevationMinimum];
            if (cell.IsUnderwater || !growFeature) cell.FeatureTypeIndex = 0;
            else {
                float randVal = Random.value;
                if (cell.TerrainTypeIndex == 0) { // sand
                    if (randVal < 0.07f) cell.FeatureTypeIndex = 2; // tank
                    else if (randVal < 0.1f) cell.FeatureTypeIndex = 3; // board
                    else if (randVal < 0.12f) cell.FeatureTypeIndex = 4; // table;
                    else cell.FeatureTypeIndex = 0;
                }
                else if (cell.TerrainTypeIndex == 1) { // grass
                    if (randVal < 0.2f) cell.FeatureTypeIndex = 1; // tree
                    else if (randVal < 0.22f) cell.FeatureTypeIndex = 4; // table
                    else cell.FeatureTypeIndex = 0;
                }
                else cell.FeatureTypeIndex = 0;
            }
        }
    }

    void CreateSpecialCell()
    {
        CreateWall();
        CreateBirth();
    }


    void CreateBirth()
    {
        int xMin = mapSizeX / 2 - 3;
        int xMax = mapSizeX / 2 + 3;
        int middle = mapSizeX / 2;

        SetWall(xMin, 1);
        SetWall(xMax, 1);
        SetWall(xMin, 2);
        SetWall(xMax, 2);
        SetWall(xMin, 3);
        SetWall(xMax, 3);
        SetWall(xMin, 4);
        SetWall(xMax, 4);
        SetWall(xMin + 1, 4);
        SetWall(xMax - 1, 4);
        SetWall(xMin + 2, 4);
        SetWall(xMax - 2, 4);

        int curElevation = elevationMinimum;
        for (int i = 1; i < 4; i++) {
            SetFloor(xMin + 1, i, curElevation);
            SetFloor(xMax - 1, i, curElevation);
            SetFloor(xMin + 2, i, curElevation);
            SetFloor(xMax - 2, i, curElevation);
            SetFloor(middle, i, curElevation);
        }
        SetFloor(middle, 4, curElevation);

        int curZ = 4;
        Cell neighbor_n = grid.GetCell(middle, curZ + 1);
        while (neighbor_n.IsUnderwater || curElevation <= neighbor_n.Elevation) {
            curElevation += 1;
            curZ += 1;
            SetWall(middle - 1, curZ);
            SetWall(middle + 1, curZ);
            SetFloor(middle, curZ, curElevation);

            neighbor_n = grid.GetCell(middle, curZ + 1);
        }
        genCellPos(middle, 2);

        // test for debug
        getBirthPos();
        getEnemyPos();
    }

    void genCellPos(int bx, int bz)
    {
        birthCell = grid.GetCell(bx, bz);
        for (int i = 0; i < 10; i++) {
            Cell newCell = GetRandomCell();
            bool isNew = true;
            foreach (Cell c in cellList) {
                if (c == newCell) {
                    isNew = false;
                    break;
                }
            }
            if (isNew) {
                cellList.Add(GetRandomCell());
            }
            else {
                i--;
            }
        }
    }

    public Vector3 getBirthPos()
    {
        Debug.Log("birth pos: " + birthCell.Position);
        return birthCell.Position;
    }

    public List<Vector3> getEnemyPos()
    {
        List<Vector3> posList = new List<Vector3>();
        foreach (Cell ec in cellList) {
            Debug.Log("enemy pos: " + ec.Position);
            posList.Add(ec.Position);
        }
        return posList;
    }

    void SetWall(int x, int z)
    {
        Cell cell = grid.GetCell(x, z);
        cell.Elevation = elevationMaximum;
        cell.TerrainTypeIndex = 3;
        cell.FeatureTypeIndex = 0;
        cell.WaterLevel = elevationMinimum;
    }

    void SetFloor(int x, int z, int e)
    {
        Cell cell = grid.GetCell(x, z);
        cell.Elevation = e;
        cell.TerrainTypeIndex = 3;
        cell.FeatureTypeIndex = 0;
        cell.WaterLevel = elevationMinimum;
    }

    void CreateWall()
    {
        for (int x = 0; x < mapSizeX; x++) {
            for (int z = 0; z < mapSizeZ; z++) {
                if (x == 0) SetWall(x, z);
                else if (x == mapSizeX - 1) SetWall(x, z);
                else if (z == 0) SetWall(x, z);
                else if (z == mapSizeZ - 1) SetWall(x, z);
            }
        }
    }

    Cell GetRandomCell()
    {
        return grid.GetCell(Random.Range(0, cellCount));
    }
}
