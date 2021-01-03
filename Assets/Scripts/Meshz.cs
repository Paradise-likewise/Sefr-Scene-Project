using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEditor;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Meshz : MonoBehaviour
{
    Mesh meshz;
    public bool useCollider, useColor, useUVCoordinates, useTerrainTypes, useNavSurface;
    [NonSerialized] List<Vector3> vertices;
    /* color 对三个顶点插值，terrainTypes 决定具体采样哪三个纹理 */
    [NonSerialized] List<Color> colors;
    [NonSerialized] List<Vector4> terrainTypes;
    [NonSerialized] List<int> triangles;
    [NonSerialized] List<Vector2> uvs;

    MeshCollider meshCollider;
    NavMeshSurface navSurface;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = meshz = new Mesh();
        if (useCollider) {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        if (useNavSurface) {
            navSurface = gameObject.AddComponent<NavMeshSurface>();
            navSurface.AddData();
        }
        meshz.name = "meshz";
    }

    public void Init()
    {
        meshz.Clear();
        vertices = ListPool<Vector3>.Get();
        if (useColor) {
            colors = ListPool<Color>.Get();
        }
        if (useTerrainTypes) {
            terrainTypes = ListPool<Vector4>.Get();
        }
        triangles = ListPool<int>.Get();
        if (useUVCoordinates) {
            uvs = ListPool<Vector2>.Get();
        }
    }

    public void Apply()
    {
        meshz.SetVertices(vertices);
        ListPool<Vector3>.Add(vertices);
        if (useColor) {
            meshz.SetColors(colors);
            ListPool<Color>.Add(colors);
        }
        if (useTerrainTypes) {
            meshz.SetUVs(1, terrainTypes);
        }
        meshz.SetTriangles(triangles, 0);
        ListPool<int>.Add(triangles);
        if (useUVCoordinates) {
            meshz.SetUVs(0, uvs);
            ListPool<Vector2>.Add(uvs);
        }
        meshz.RecalculateNormals();
        if (useCollider) {
            meshCollider.sharedMesh = meshz;
        }
    }

    // 顺时针
    public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(Metrics.Perturb(v1));
        vertices.Add(Metrics.Perturb(v2));
        vertices.Add(Metrics.Perturb(v3));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }
    public void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }
    public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
    }
    public void AddTriangleUVFromXY(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        uvs.Add(new Vector2(v1.x, v1.y));
        uvs.Add(new Vector2(v2.x, v2.y));
        uvs.Add(new Vector2(v3.x, v3.y));
    }
    public void AddTriangleUVFromZY(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        uvs.Add(new Vector2(v1.z, v1.y));
        uvs.Add(new Vector2(v2.z, v2.y));
        uvs.Add(new Vector2(v3.z, v3.y));
    }
    public void AddTriangleTerrainTypes(Vector4 types)
    {
        terrainTypes.Add(types);
        terrainTypes.Add(types);
        terrainTypes.Add(types);
    }


    public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(Metrics.Perturb(v1));
        vertices.Add(Metrics.Perturb(v2));
        vertices.Add(Metrics.Perturb(v3));
        vertices.Add(Metrics.Perturb(v4));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }
    public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }
    public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
        uvs.Add(uv4);
    }
    public void AddQuadUV(float uMin, float uMax, float vMin, float vMax)
    {
        uvs.Add(new Vector2(uMin, vMin));
        uvs.Add(new Vector2(uMax, vMin));
        uvs.Add(new Vector2(uMin, vMax));
        uvs.Add(new Vector2(uMax, vMax));
    }
    public void AddQuadUVFromXZ(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        uvs.Add(new Vector2(v1.x, v1.z));
        uvs.Add(new Vector2(v2.x, v2.z));
        uvs.Add(new Vector2(v3.x, v3.z));
        uvs.Add(new Vector2(v4.x, v4.z));
    }
    public void AddQuadUVFromXY(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        uvs.Add(new Vector2(v1.x, v1.y));
        uvs.Add(new Vector2(v2.x, v2.y));
        uvs.Add(new Vector2(v3.x, v3.y));
        uvs.Add(new Vector2(v4.x, v4.y));
    }
    public void AddQuadUVFromZY(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        uvs.Add(new Vector2(v1.z, v1.y));
        uvs.Add(new Vector2(v2.z, v2.y));
        uvs.Add(new Vector2(v3.z, v3.y));
        uvs.Add(new Vector2(v4.z, v4.y));
    }
    
    public void AddQuadTerrainTypes(Vector4 types)
    {
        terrainTypes.Add(types);
        terrainTypes.Add(types);
        terrainTypes.Add(types);
        terrainTypes.Add(types);
    }

    public void Save(string filename)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
    }
}
