using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTerrainChunk
{
    private GameObject _terrain;
    private Vector2Int _chunkPos;
    private float _detail;

    public WorldTerrainChunk(Vector2Int chunkPos, float[,] heightMap)
    {
        this._chunkPos = chunkPos;
        ApplyHeightMap(heightMap);
    }

    public void ApplyHeightMap(float[,] heightMap)
    {
        if (_terrain) GameObject.Destroy(_terrain);

        int chunkSize = WorldGenerator.Instance.GetChunkSize();

        _terrain = TerrainGenerator.GenerateFlatShaded(heightMap);
        _terrain.transform.localScale = new Vector3(chunkSize / (heightMap.GetLength(0) - 1), 1, chunkSize / (heightMap.GetLength(1) - 1));
        _terrain.transform.position = new Vector3(_chunkPos.x * chunkSize, 0, _chunkPos.y * chunkSize);

        _detail = (heightMap.GetLength(0) - 1) / chunkSize; 
    }

    public float GetDetail()
    {
        return _detail;
    }
}
