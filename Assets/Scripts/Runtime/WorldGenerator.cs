using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance;

    private const int MAX_GENERATED_TILES = 10000;

    [Header("Dimensions")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [Space]
    [SerializeField] private int _chunkSize;

    [Header("Noise Settings")]
    [SerializeField] private int _seed;
    [SerializeField] private float _scale;
    [SerializeField] [Range(1, 16)] private int _octaves;
    [SerializeField] private Vector2 _offset;
    [SerializeField] private float multiplier;

    [Header("References")]
    [SerializeField] private Material material;

    private List<WorldTerrainChunk> _chunks;
    
    private float _prevDetail = 0.001f;
    private Vector2Int _prevStartChunkPos;
    private Vector2Int _prevVisibleChunksCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Generate();
    }

    private void Update()
    {
        GenerateVisibleChunks();
    }

    public void Generate()
    {
        Random.InitState(_seed);
        _chunks = new List<WorldTerrainChunk>();

        TerrainGenerator.SetMaterial(material);

        GenerateAllChunks();
    }

    private void GenerateAllChunks()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                // Vector2 offset = _offset + new Vector2(x * _chunkSize, y * _chunkSize);

                float[,] heightMap = GenerateHeightMap(new Vector2Int(x, y), 0.001f);

                _chunks.Add(new WorldTerrainChunk(new Vector2Int(x, y), heightMap));
            }
        }
    }

    private void GenerateVisibleChunks()
    {
        float currentDetail = CalcDetailFromVisibleChunksCount();
        Vector2Int startChunkPos = CalcBottomLeftCornerVisibleChunkPos();
        Vector2Int visibleChunksCount = CalcVisibleChunksCount();

        if (_prevDetail == currentDetail && _prevStartChunkPos == startChunkPos) return;

        // Unload previously loaded chunks
        for (int y = 0, i = 0; y < _prevVisibleChunksCount.y; y++)
        {
            for (int x = 0; x < _prevVisibleChunksCount.x; x++, i++)
            {
                // Don't unload the chunks that are visible now.
                if (x >= startChunkPos.x && x < startChunkPos.x + x)
                    if (y >= startChunkPos.y && y < startChunkPos.y + y) continue;

                WorldTerrainChunk chunk = _chunks[((_prevStartChunkPos.y + y) * _width) + _prevStartChunkPos.x + x];
                float[,] heightMap = GenerateHeightMap(_prevStartChunkPos + new Vector2Int(x, y), 0.001f);

                chunk.ApplyHeightMap(heightMap);
            }
        }

        _prevStartChunkPos = startChunkPos;
        _prevVisibleChunksCount = visibleChunksCount;

        // Load new chunks
        for (int y = 0, i = 0; y < visibleChunksCount.y; y++)
        {
            for (int x = 0; x < visibleChunksCount.x; x++, i++)
            {
                // if (x >= _prevStartChunkPos.x && x < _prevStartChunkPos.x + x)
                    // if (y >= _prevStartChunkPos.y && y < _prevStartChunkPos.y + y) continue;

                WorldTerrainChunk chunk = _chunks[((startChunkPos.y + y) * _width) + startChunkPos.x + x];
                float[,] heightMap = GenerateHeightMap(startChunkPos + new Vector2Int(x, y), currentDetail);

                if (chunk.GetDetail() == currentDetail) continue;

                chunk.ApplyHeightMap(heightMap);
            }
        }

        _prevDetail = currentDetail;
    }

    private float[,] GenerateHeightMap(Vector2Int chunkPos, float detail)
    {
        int visibleChunkWidth = Mathf.FloorToInt((float)_chunkSize * detail);
        int visibleChunkHeight = Mathf.FloorToInt((float)_chunkSize * detail);

        float[,] heightMap = new float[visibleChunkWidth + 1, visibleChunkHeight + 1];

        float chunkXT = Mathf.InverseLerp(0, _chunkSize, (float)_chunkSize / visibleChunkWidth);
        float chunkYT = Mathf.InverseLerp(0, _chunkSize, (float)_chunkSize / visibleChunkHeight);

        for (int tileY = 0; tileY <= visibleChunkHeight; tileY++)
        {
            for (int tileX = 0; tileX <= visibleChunkWidth; tileX++)
            {
                int currTileX = Mathf.FloorToInt(_chunkSize * Mathf.Clamp01(tileX * chunkXT));
                int currTileY = Mathf.FloorToInt(_chunkSize * Mathf.Clamp01(tileY * chunkYT));

                Vector2 offset = _offset + new Vector2Int(chunkPos.x * _chunkSize, chunkPos.y * _chunkSize);

                float noiseVal = PerlinNoise.GeneratePerlinNoise(1, 1, _seed, _scale, _octaves, offset + new Vector2(currTileX, currTileY), 0.5f, 2, 0, PerlinNoise.NormalizeMode.Global)[0, 0];
                heightMap[tileX, tileY] = noiseVal * multiplier;
            }
        }

        return heightMap;
    }

    private Vector2Int CalcBottomLeftCornerVisibleChunkPos()
    {
        Vector3 start = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.transform.position.y));

        // Clamp to borders
        start = new Vector3(Mathf.Clamp(start.x, 0, _width *_chunkSize), 0, Mathf.Clamp(start.z, 0, _height *_chunkSize));

        return new Vector2Int(Mathf.FloorToInt(start.x / _chunkSize), Mathf.FloorToInt(start.z / _chunkSize));
    }

    private Vector2Int CalcVisibleChunksCount()
    {
        Vector3 start = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.transform.position.y));
        Vector3 end = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.transform.position.y));

        // Clamp to borders
        start = new Vector3(Mathf.Clamp(start.x, 0, _width *_chunkSize), 0, Mathf.Clamp(start.z, 0, _height *_chunkSize));
        end = new Vector3(Mathf.Clamp(end.x, 0, _width *_chunkSize), 0, Mathf.Clamp(end.z, 0, _height *_chunkSize));

        Vector3 size = end - start;
        Vector2Int chunksSize = new Vector2Int(Mathf.CeilToInt(size.x / _chunkSize), Mathf.CeilToInt(size.z / _chunkSize));
        chunksSize = new Vector2Int(Mathf.Clamp(chunksSize.x + 1, 0, _width), Mathf.Clamp(chunksSize.y + 1, 0, _height));

        return chunksSize;
    }

    private float CalcDetailFromVisibleChunksCount()
    {
        float currZoom = Camera.main.transform.position.y;
        // return (1 / (float)(CalcVisibleChunksCount().x * CalcVisibleChunksCount().y)) * (MAX_GENERATED_TILES / _chunkSize);
        return 0.001f * (60000 / currZoom);
    }

    #region Get Methods

    public int GetWidth()
    {
        return _width;
    }

    public int GetHeight()
    {
        return _height;
    }

    public int GetChunkSize()
    {
        return _chunkSize;
    }

    #endregion

    // private float[,] GenerateHeightMap()
    // {
    //     float[,] heightMap = new float[_width, _height];

    //     int visibleChunkWidth = Mathf.FloorToInt(_chunkSize * _detail);
    //     int visibleChunkHeight = Mathf.FloorToInt(_chunkSize * _detail);

    //     float chunkXT = Mathf.InverseLerp(0, _chunkSize, _chunkSize / visibleChunkWidth);
    //     float chunkYT = Mathf.InverseLerp(0, _chunkSize, _chunkSize / visibleChunkHeight);

    //     for (int worldY = 0; worldY < _height; worldY += visibleChunkHeight)
    //     {
    //         for (int worldX = 0; worldX < _width; worldX += visibleChunkWidth)
    //         {
    //             Vector2 offset = _offset + new Vector2Int((worldX / visibleChunkWidth) * _chunkSize, (worldY / visibleChunkHeight) * _chunkSize);
                
    //             for (int chunkY = 0; chunkY < visibleChunkHeight; chunkY++)
    //             {
    //                 for (int chunkX = 0; chunkX < visibleChunkWidth; chunkX++)
    //                 {
    //                     int currChunkX = Mathf.FloorToInt(_chunkSize * (chunkX * chunkXT));
    //                     int currChunkY = Mathf.FloorToInt(_chunkSize * (chunkY * chunkYT));

    //                     if (worldX + chunkX >= _width) break;
    //                     if (worldY + chunkY >= _height) return heightMap;

    //                     float[,] noise = PerlinNoise.GeneratePerlinNoise(1, 1, _seed, _scale, _octaves, offset + new Vector2(currChunkX, currChunkY), 0.5f, 2, 0, PerlinNoise.NormalizeMode.Global);

    //                     heightMap[worldX + chunkX, worldY + chunkY] = noise[0, 0] * multiplier;
    //                 }
    //             }
    //         }
    //     }

    //     return heightMap;
    // }

}