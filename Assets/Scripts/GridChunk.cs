using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridChunk : MonoBehaviour
{
    Cell[] cells;
    Canvas gridCanvas;
    public Meshz terrain, water, waterShore;
    public FeatureManager featureManager;

    static Color color1 = new Color(1f, 0f, 0f, 0f);
    static Color color2 = new Color(0f, 1f, 0f, 0f);
    static Color color3 = new Color(0f, 0f, 1f, 0f);
    static Color color4 = new Color(0f, 0f, 0f, 1f);

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        cells = new Cell[Metrics.chunkSizeZ * Metrics.chunkSizeX];
    }

    public void AddCell(int localX, int localZ, Cell cell)
    {
        cells[localX + localZ * Metrics.chunkSizeX] = cell;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
        cell.chunk = this;
    }

    public void Refresh()
    {
        enabled = true; // default is true
    }

    private void LateUpdate()
    {
        Triangulate();
        enabled = false;
    }

    public void Triangulate()
    {
        terrain.Init();
        water.Init();
        waterShore.Init();
        featureManager.Init();
        for (int i = 0; i < cells.Length; i++) {
            Triangulate(cells[i]);
        }
        terrain.Apply();
        water.Apply();
        waterShore.Apply();
        featureManager.Apply();
    }

    public void Triangulate(Cell cell)
    {
        Vector3 center = cell.Position;
        Vector3 v1 = center + Metrics.corners[0] * Metrics.solidFactor;
        Vector3 v2 = center + Metrics.corners[3] * Metrics.solidFactor;
        Vector3 v3 = center + Metrics.corners[1] * Metrics.solidFactor;
        Vector3 v4 = center + Metrics.corners[2] * Metrics.solidFactor;
        terrain.AddQuad(v1, v2, v3, v4);
        terrain.AddQuadUVFromXZ(v1, v2, v3, v4);
        // terrain.AddQuadColor(cell.Color, cell.Color, cell.Color, cell.Color);
        terrain.AddQuadColor(color1, color1, color1, color1);
        terrain.AddQuadTerrainTypes(new Vector4(cell.TerrainTypeIndex, 0f, 0f, 0f));

        if (cell.IsUnderwater) {
            Vector3 water_center = center;
            water_center.y = cell.WaterSurfaceY;
            water.AddQuad(
                water_center + Metrics.corners[0] * Metrics.solidFactor,
                water_center + Metrics.corners[3] * Metrics.solidFactor,
                water_center + Metrics.corners[1] * Metrics.solidFactor,
                water_center + Metrics.corners[2] * Metrics.solidFactor);
        }

        for (Direction d = Direction.W; d < Direction.E; d++) {
            TriangulateConnection(d, cell);
            if (cell.IsUnderwater) TriangulateWaterConnection(d, cell);
        }
        for (Direction d = Direction.W; d <= Direction.S; d++) {
            if (cell.IsUnderwater) TriangulateWaterShore(d, cell);
        }

        featureManager.AddFeature(cell);
    }

    Cell findMinCell(Cell cell1, Cell cell2, Cell cell3, Cell cell4)
    {
        Cell min_cell;
        if (cell1.Elevation <= cell2.Elevation) {
            if (cell1.Elevation <= cell3.Elevation) {
                if (cell1.Elevation <= cell4.Elevation) min_cell = cell1;
                else min_cell = cell4;
            }
            else {
                if (cell3.Elevation <= cell4.Elevation) min_cell = cell3;
                else min_cell = cell4;
            }
        }
        else {
            if (cell2.Elevation <= cell3.Elevation) {
                if (cell2.Elevation <= cell4.Elevation) min_cell = cell2;
                else min_cell = cell4;
            }
            else {
                if (cell3.Elevation <= cell4.Elevation) min_cell = cell3;
                else min_cell = cell4;
            }
        }
        return min_cell;
    }

    void TriangulateConnection(Direction direction, Cell cell)
    {
        // edge
        Cell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null) return;

        int d = (int)direction;
        int next_d = (d + 1) % 4;

        Vector3 v1 = Metrics.corners[d];
        Vector3 v2 = Metrics.corners[next_d];
        Vector3 bridge = (v1 + v2) * (1 - Metrics.solidFactor);

        Vector3 center = cell.Position;
        v1 = v1 * Metrics.solidFactor + center;
        v2 = v2 * Metrics.solidFactor + center;
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        v3.y = v4.y = neighbor.Position.y;

        int t1 = cell.TerrainTypeIndex, t2 = neighbor.TerrainTypeIndex;
        if (cell.GetEdgeType(direction) == HexEdgeType.Flat) {
            terrain.AddQuad(v1, v2, v3, v4);
            terrain.AddQuadUVFromXZ(v1, v2, v3, v4);
            //terrain.AddQuadColor(cell.Color, cell.Color, neighbor.Color, neighbor.Color);
            terrain.AddQuadColor(color1, color1, color2, color2);
            terrain.AddQuadTerrainTypes(new Vector4(t1, t2, 0f, 0f));
        }
        else if (cell.GetEdgeType(direction) == HexEdgeType.Slope) {
            if (cell.Elevation < neighbor.Elevation) {
                if (d % 2 == 0) TriangulatePosSlope(v1, v2, cell, v3, v4, neighbor, false);
                else TriangulatePosSlope(v1, v2, cell, v3, v4, neighbor, true);
            }
            else {
                if (d % 2 == 0) TriangulatePosSlope(v4, v3, neighbor, v2, v1, cell, false);
                else TriangulatePosSlope(v4, v3, neighbor, v2, v1, cell, true);
            }
        }
        else if (cell.GetEdgeType(direction) == HexEdgeType.Cliff) {
            int delta = neighbor.Elevation - cell.Elevation;
            if (delta > 0) {
                Vector3 v5 = v3;
                Vector3 v6 = v4;
                v5.y -= delta * Metrics.elevationStep;
                v6.y -= delta * Metrics.elevationStep;

                terrain.AddQuad(v1, v2, v5, v6);
                terrain.AddQuadUVFromXZ(v1, v2, v5, v6);
                //terrain.AddQuadColor(cell.Color, cell.Color, cell.Color, cell.Color);
                terrain.AddQuadColor(color1, color1, color2, color2);
                terrain.AddQuadTerrainTypes(new Vector4(t1, t2, 0f, 0f));

                terrain.AddQuad(v5, v6, v3, v4);
                if (d % 2 == 0) terrain.AddQuadUVFromZY(v5, v6, v3, v4);
                else terrain.AddQuadUVFromXY(v5, v6, v3, v4);
                // terrain.AddQuadColor(cell.Color, cell.Color, neighbor.Color, neighbor.Color);
                terrain.AddQuadColor(color2, color2, color2, color2);
                terrain.AddQuadTerrainTypes(new Vector4(0f, t2, 0f, 0f));
            }
            else {
                Vector3 v5 = v1;
                Vector3 v6 = v2;
                v5.y += delta * Metrics.elevationStep;
                v6.y += delta * Metrics.elevationStep;

                terrain.AddQuad(v1, v2, v5, v6);
                if (d % 2 == 0) terrain.AddQuadUVFromZY(v1, v2, v5, v6);
                else terrain.AddQuadUVFromXY(v1, v2, v5, v6);
                //terrain.AddQuadColor(cell.Color, cell.Color, neighbor.Color, neighbor.Color);
                terrain.AddQuadColor(color1, color1, color1, color1);
                terrain.AddQuadTerrainTypes(new Vector4(t1, 0f, 0f, 0f));
                
                terrain.AddQuad(v5, v6, v3, v4);
                terrain.AddQuadUVFromXZ(v5, v6, v3, v4);
                //terrain.AddQuadColor(neighbor.Color, neighbor.Color, neighbor.Color, neighbor.Color);
                terrain.AddQuadColor(color1, color1, color2, color2);
                terrain.AddQuadTerrainTypes(new Vector4(t1, t2, 0f, 0f));
            }
        }

        // corner
        if (direction != Direction.W) return;
        Cell prev_neighbor = cell.GetNeighbor(Direction.S);
        if (prev_neighbor == null) return;

        Cell corner_cell = neighbor.GetNeighbor(Direction.S);
        Vector3 bridge2 = (Metrics.corners[0] + Metrics.corners[3]) * (1 - Metrics.solidFactor);
        v2 = v1 + bridge;
        v3 = v1 + bridge2;
        v4 = v3 + bridge;
        v2.y = neighbor.Position.y;
        v3.y = prev_neighbor.Position.y;
        v4.y = corner_cell.Position.y;

        Cell min_cell = findMinCell(cell, neighbor, prev_neighbor, corner_cell);
        if (min_cell == cell) {
            TriangulateCorner(v1, cell, v2, neighbor, v4, corner_cell, v3, prev_neighbor, false);
        }
        else if (min_cell == neighbor) {
            TriangulateCorner(v2, neighbor, v4, corner_cell, v3, prev_neighbor, v1, cell, true);
        }
        else if (min_cell == prev_neighbor) {
            TriangulateCorner(v3, prev_neighbor, v1, cell, v2, neighbor, v4, corner_cell, true);
        }
        else {
            TriangulateCorner(v4, corner_cell, v3, prev_neighbor, v1, cell, v2, neighbor, false);
        }
    }

    void TriangulateWaterConnection(Direction direction, Cell cell)
    {
        // edge
        Vector3 center = cell.Position;
        center.y = cell.WaterSurfaceY;
        Cell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null || !neighbor.IsUnderwater) {
            return;
        }

        int d = (int)direction;
        int next_d = (d + 1) % 4;

        Vector3 bridge = (Metrics.corners[d] + Metrics.corners[next_d]) * (1 - Metrics.solidFactor);
        Vector3 v1 = center + Metrics.corners[d] * Metrics.solidFactor;
        Vector3 v2 = center + Metrics.corners[next_d] * Metrics.solidFactor;
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        water.AddQuad(v1, v2, v3, v4);

        // corner
        if (direction != Direction.W) return;

        Cell prev_neighbor = cell.GetNeighbor(Direction.S);
        if (prev_neighbor == null || !prev_neighbor.IsUnderwater) return;

        Cell corner_cell = neighbor.GetNeighbor(Direction.S);
        if (!corner_cell.IsUnderwater) return;

        Vector3 bridge2 = (Metrics.corners[0] + Metrics.corners[3]) * (1 - Metrics.solidFactor);
        v2 = v3;
        v3 = v1 + bridge2;
        v4 = v3 + bridge;
        water.AddQuad(v1, v2, v3, v4);
    }

    void TriangulateWaterShore(Direction direction, Cell cell)
    {
        // edge
        Vector3 center = cell.Position;
        center.y = cell.WaterSurfaceY;
        Cell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null) return;

        int d = (int)direction;
        int next_d = (d + 1) % 4;
        Vector3 bridge = (Metrics.corners[d] + Metrics.corners[next_d]) * (1 - Metrics.solidFactor);
        Vector3 v1 = center + Metrics.corners[d] * Metrics.solidFactor;
        Vector3 v2 = center + Metrics.corners[next_d] * Metrics.solidFactor;
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        if (!neighbor.IsUnderwater) {
            waterShore.AddQuad(v1, v2, v3, v4);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
        }

        // corner
        int prev_d = (d + 3) % 4;
        Direction prev_direction = (Direction)(prev_d);
        Cell prev_neighbor = cell.GetNeighbor(prev_direction);
        if (prev_neighbor == null) return;

        Cell corner_cell = neighbor.GetNeighbor(prev_direction);

        // 0: no, 1: ` ., 2: | , 3: |_, 4: .
        Vector3 bridge2 = (Metrics.corners[d] + Metrics.corners[prev_d]) * (1 - Metrics.solidFactor);
        v2 = v1 + bridge;
        v3 = v1 + bridge2;
        v4 = v3 + bridge;

        int case_index = 0;
        if (!corner_cell.IsUnderwater) {
            if (!neighbor.IsUnderwater) {
                if (!prev_neighbor.IsUnderwater) case_index = 3;
                else case_index = 2;
            }
            else if (prev_neighbor.IsUnderwater) {
                case_index = 4;
            }
        }
        else if (!neighbor.IsUnderwater && 
                !prev_neighbor.IsUnderwater && 
                direction < Direction.S) {
            case_index = 1;
        }

        if (case_index == 2) {
            waterShore.AddQuad(v3, v1, v4, v2);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
        }
        else if (case_index == 3) {
            waterShore.AddQuad(v1, v2, v3, v4);
            waterShore.AddQuadUV(
                new Vector2(0f, 0f), 
                new Vector2(0f, 1f), 
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));
        }
        else if (case_index == 4) {
            waterShore.AddQuad(v1, v2, v3, v4);
            waterShore.AddQuadUV(
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 1f));
        }
        else if (case_index == 1) {
            waterShore.AddQuad(v1, v2, v3, v4);
            waterShore.AddQuadUV(
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 0f));
        }
    }

    void TriangulatePosSlope(
        Vector3 beginLeft, Vector3 beginRight, Cell beginCell,
        Vector3 endLeft, Vector3 endRight, Cell endCell, bool is_NS_direction
    )
    {
        int t1 = beginCell.TerrainTypeIndex, t2 = endCell.TerrainTypeIndex;
        Vector3 v3 = Metrics.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 v4 = Metrics.TerraceLerp(beginRight, endRight, 1);
        // Color c2 = Metrics.TerraceLerp(beginCell.Color, endCell.Color, 1);
        Color c2 = Metrics.TerraceLerp(color1, color2, 1);
        terrain.AddQuad(beginLeft, beginRight, v3, v4);
        terrain.AddQuadUVFromXZ(beginLeft, beginRight, v3, v4);
        //terrain.AddQuadColor(beginCell.Color, beginCell.Color, c2, c2);
        terrain.AddQuadColor(color1, color1, c2, c2);
        terrain.AddQuadTerrainTypes(new Vector4(t1, t2, 0f, 0f));

        Vector3 endLeftBelow = endLeft, endRightBelow = endRight;
        endLeftBelow.y -= (endCell.Elevation - beginCell.Elevation) * Metrics.elevationStep;
        endRightBelow.y -= (endCell.Elevation - beginCell.Elevation) * Metrics.elevationStep;
        terrain.AddTriangle(beginLeft, endLeftBelow, v3);
        if (is_NS_direction) terrain.AddTriangleUVFromZY(beginLeft, endLeftBelow, v3);
        else terrain.AddTriangleUVFromXY(beginLeft, endLeftBelow, v3);
        //terrain.AddTriangleColor(beginCell.Color, beginCell.Color, c2);
        terrain.AddTriangleColor(color1, color2, c2);
        terrain.AddTriangleTerrainTypes(new Vector4(t1, t2, 0f, 0f));
        
        terrain.AddTriangle(v4, endRightBelow, beginRight);
        if (is_NS_direction) terrain.AddTriangleUVFromZY(v4, endRightBelow, beginRight);
        else terrain.AddTriangleUVFromXY(v4, endRightBelow, beginRight);
        //terrain.AddTriangleColor(c2, beginCell.Color, beginCell.Color);
        terrain.AddTriangleColor(c2, color2, color1);
        terrain.AddTriangleTerrainTypes(new Vector4(t1, t2, 0f, 0f));

        for (int i = 2; i < Metrics.terracesSteps; i++) {
            Vector3 v1 = v3, v2 = v4;
            Color c1 = c2;
            v3 = Metrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = Metrics.TerraceLerp(beginRight, endRight, i);
            //c2 = Metrics.TerraceLerp(beginCell.Color, endCell.Color, i);
            c2 = Metrics.TerraceLerp(c1, c2, i);
            terrain.AddQuad(v1, v2, v3, v4);
            terrain.AddQuadUVFromXZ(v1, v2, v3, v4);
            //terrain.AddQuadColor(c1, c1, c2, c2);
            terrain.AddQuadColor(c1, c1, c2, c2);
            terrain.AddQuadTerrainTypes(new Vector4(t1, t2, 0f, 0f));

            terrain.AddTriangle(v1, endLeftBelow, v3);
            if (is_NS_direction) terrain.AddTriangleUVFromZY(v1, endLeftBelow, v3);
            else terrain.AddTriangleUVFromXY(v1, endLeftBelow, v3);
            //terrain.AddTriangleColor(c1, beginCell.Color, c2);
            terrain.AddTriangleColor(c1, color2, c2);
            terrain.AddTriangleTerrainTypes(new Vector4(t1, t2, 0f, 0f));

            terrain.AddTriangle(v4, endRightBelow, v2);
            if (is_NS_direction) terrain.AddTriangleUVFromZY(v4, endRightBelow, v2);
            else terrain.AddTriangleUVFromXY(v4, endRightBelow, v2);
            //terrain.AddTriangleColor(c2, beginCell.Color, c1);
            terrain.AddTriangleColor(c2, color2, c1);
            terrain.AddTriangleTerrainTypes(new Vector4(t1, t2, 0f, 0f));
        }
        terrain.AddQuad(v3, v4, endLeft, endRight);
        terrain.AddQuadUVFromXZ(v3, v4, endLeft, endRight);
        //terrain.AddQuadColor(c2, c2, endCell.Color, endCell.Color);
        terrain.AddQuadColor(c2, c2, color2, color2);
        terrain.AddQuadTerrainTypes(new Vector4(t1, t2, 0f, 0f));

        terrain.AddTriangle(v3, endLeftBelow, endLeft);
        if (is_NS_direction) terrain.AddTriangleUVFromZY(v3, endLeftBelow, endLeft);
        else terrain.AddTriangleUVFromXY(v3, endLeftBelow, endLeft);
        //terrain.AddTriangleColor(c2, beginCell.Color, endCell.Color);
        terrain.AddTriangleColor(c2, color2, color2);
        terrain.AddTriangleTerrainTypes(new Vector4(t1, t2, 0f, 0f));

        terrain.AddTriangle(endRight, endRightBelow, v4);
        if (is_NS_direction) terrain.AddTriangleUVFromZY(endRight, endRightBelow, v4);
        else terrain.AddTriangleUVFromXY(endRight, endRightBelow, v4);
        //terrain.AddTriangleColor(endCell.Color, beginCell.Color, c2);
        terrain.AddTriangleColor(color2, color2, c2);
        terrain.AddTriangleTerrainTypes(new Vector4(t1, t2, 0f, 0f));
    }

    void TriangulateCorner(
        Vector3 v1, Cell cell1, Vector3 v2, Cell cell2,
        Vector3 v3, Cell cell3, Vector3 v4, Cell cell4, bool is_NS_direction)
    {
        Vector3 v21 = v2, v31 = v3, v41 = v4;
        //Color c2 = cell2.Color, c3 = cell3.Color, c4 = cell4.Color;
        int t1 = cell1.TerrainTypeIndex, t2 = cell2.TerrainTypeIndex;
        int t3 = cell3.TerrainTypeIndex, t4 = cell4.TerrainTypeIndex;
        int delta21 = cell2.Elevation - cell1.Elevation;
        int delta31 = cell3.Elevation - cell1.Elevation;
        int delta41 = cell4.Elevation - cell1.Elevation;
        if (delta21 > 0) {
            v21.y -= delta21 * Metrics.elevationStep;
            //t2 = cell1.TerrainTypeIndex;
        }
        if (delta31 > 0) {
            v31.y -= delta31 * Metrics.elevationStep;
            //t3 = cell1.TerrainTypeIndex;
            if (delta31 < delta21) {
                Vector3 v22 = v2;
                v22.y -= (delta21 - delta31) * Metrics.elevationStep;
                terrain.AddQuad(v31, v21, v3, v22);
                if (is_NS_direction) terrain.AddQuadUVFromXY(v31, v21, v3, v22);
                else terrain.AddQuadUVFromZY(v31, v21, v3, v22);
                //terrain.AddQuadColor(cell1.Color, cell1.Color, cell3.Color, cell3.Color);
                terrain.AddQuadColor(color1, color2, color1, color2);
                terrain.AddQuadTerrainTypes(new Vector4(t3, t2, 0f, 0f));
            }
            else if (delta21 > 0) {
                Vector3 v32 = v3;
                v32.y -= (delta31 - delta21) * Metrics.elevationStep;
                terrain.AddQuad(v31, v21, v32, v2);
                if (is_NS_direction) terrain.AddQuadUVFromXY(v31, v21, v32, v2);
                else terrain.AddQuadUVFromZY(v31, v21, v32, v2);
                //terrain.AddQuadColor(cell1.Color, cell1.Color, cell2.Color, cell2.Color);
                terrain.AddQuadColor(color1, color2, color1, color2);
                terrain.AddQuadTerrainTypes(new Vector4(t3, t2, 0f, 0f));
            }
        }
        if (delta41 > 0) {
            v41.y -= delta41 * Metrics.elevationStep;
            //t4 = cell1.TerrainTypeIndex;
            if (delta41 < delta31) {
                Vector3 v32 = v3;
                v32.y -= (delta31 - delta41) * Metrics.elevationStep;
                terrain.AddQuad(v41, v31, v4, v32);
                if (is_NS_direction) terrain.AddQuadUVFromZY(v41, v31, v4, v32);
                else terrain.AddQuadUVFromXY(v41, v31, v4, v32);
                //terrain.AddQuadColor(cell1.Color, cell1.Color, cell4.Color, cell4.Color);
                terrain.AddQuadColor(color1, color2, color1, color2);
                terrain.AddQuadTerrainTypes(new Vector4(t4, t3, 0f, 0f));
            }
            else if (delta31 > 0) {
                Vector3 v42 = v4;
                v42.y -= (delta41 - delta31) * Metrics.elevationStep;
                terrain.AddQuad(v41, v31, v42, v3);
                if (is_NS_direction) terrain.AddQuadUVFromZY(v41, v31, v42, v3);
                else terrain.AddQuadUVFromXY(v41, v31, v42, v3);
                //terrain.AddQuadColor(cell1.Color, cell1.Color, cell3.Color, cell3.Color);
                terrain.AddQuadColor(color1, color2, color1, color2);
                terrain.AddQuadTerrainTypes(new Vector4(t4, t3, 0f, 0f));
            }
        }

        terrain.AddQuad(v1, v21, v41, v31);
        terrain.AddQuadUVFromXZ(v1, v21, v41, v31);
        //terrain.AddQuadColor(cell1.Color, c2, c4, c3);
        terrain.AddQuadColor(color1, color2, color3, color4);
        terrain.AddQuadTerrainTypes(new Vector4(t1, t2, t4, t3));
    }
}
