using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainGenerator
{
    private static Material material;

    public static GameObject GenerateFlatShaded(float[,] heightMap, float detail = 1)
    {
        int xSize = heightMap.GetLength(0) - 1;
        int zSize = heightMap.GetLength(1) - 1;
        int distPerVertex = Mathf.FloorToInt(1 / detail);

        Vector3[] vertices = new Vector3[(int)((xSize * zSize) * detail) * 6];
        int[] triangles = new int[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];

        GameObject newTerrain = new GameObject("Terrain");
        MeshRenderer mr = newTerrain.AddComponent<MeshRenderer>();
        mr.material = material;
        newTerrain.AddComponent<MeshFilter>();
        Mesh mesh = newTerrain.GetComponent<MeshFilter>().mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        for (int z = 0, i = 0; z < zSize; z += distPerVertex)
        {
            for (int x = 0; x < xSize; x += distPerVertex, i += 6)
            {
                vertices[i] = new Vector3(x, 0, z);
                vertices[i + 1] = new Vector3(x, 0, z + distPerVertex);
                vertices[i + 2] = new Vector3(x + distPerVertex, 0, z);
                vertices[i + 3] = new Vector3(x, 0, z + distPerVertex);
                vertices[i + 4] = new Vector3(x + distPerVertex, 0, z + distPerVertex);
                vertices[i + 5] = new Vector3(x + distPerVertex, 0, z);

                // Apply noise
                for (int i2 = 0; i2 < 6; i2++)
                {
                    Vector3 vertex = vertices[i + i2];
                    vertex.y = heightMap[(int)vertex.x, (int)vertex.z];
                    vertices[i + i2] = vertex;
                }

                // Triangulate
                for (int i2 = 0; i2 < 6; i2++)
                {
                    triangles[i + i2] = i + i2;
                }

                // UVs
                for (int i2 = 0; i2 < 6; i2++)
                {
                    Vector3 vertex = vertices[i + i2];
                    uv[i + i2] = new Vector2((float)vertex.x / xSize, (float)vertex.z / zSize);
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        return newTerrain;
    }

    public static GameObject GenerateSmoothShaded(float[,] heightMap)
    {
        int xSize = heightMap.GetLength(0) - 1;
        int zSize = heightMap.GetLength(1) - 1;

        GameObject newTerrain = new GameObject("Terrain");
        MeshRenderer mr = newTerrain.AddComponent<MeshRenderer>();
        mr.material = material;
        newTerrain.AddComponent<MeshFilter>();
        Mesh mesh = newTerrain.GetComponent<MeshFilter>().mesh = new Mesh();

        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
		for (int i = 0, z = 0; z <= zSize; z++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x, heightMap[x, z], z);                
                uv[i] = new Vector2((float)x / xSize, (float)z / zSize);
			}
		}

		int[] triangles = new int[xSize * zSize * 6];
		for (int ti = 0, vi = 0, z = 0; z < zSize; z++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        return newTerrain;
    }

    public static void SetMaterial(Material material)
    {
        TerrainGenerator.material = material;
    }
}
