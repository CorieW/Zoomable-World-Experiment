using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class WorldTextureGenerator : MonoBehaviour
{
    private SpriteRenderer _sr;

    [Header("Dimensions")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [Space]
    [SerializeField] private int _chunkWidth;
    [SerializeField] private int _chunkHeight;

    [Header("Noise Settings")]
    [SerializeField] private int _seed;
    [SerializeField] private float _scale;
    [SerializeField] [Range(1, 16)] private int _octaves;
    [SerializeField] [Range(0, 1)] private float _detail;

    private float[,] _heightMap;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {

    }
    
    public void Generate()
    {
        Random.InitState(_seed);
        GenerateTexture();
        GenerateHeightMap();
        DrawTexture();
    }

    private void GenerateHeightMap()
    {
        _heightMap = new float[_width, _height];

        int visibleChunkWidth = Mathf.FloorToInt(_chunkWidth * _detail);
        int visibleChunkHeight = Mathf.FloorToInt(_chunkHeight * _detail);

        float chunkXT = Mathf.InverseLerp(0, _chunkWidth, _chunkWidth / visibleChunkWidth);
        float chunkYT = Mathf.InverseLerp(0, _chunkHeight, _chunkHeight / visibleChunkHeight);

        for (int worldY = 0; worldY < _height; worldY += visibleChunkHeight)
        {
            for (int worldX = 0; worldX < _width; worldX += visibleChunkWidth)
            {
                Vector2 offset = new Vector2Int((worldX / visibleChunkWidth) * _chunkWidth, (worldY / visibleChunkHeight) * _chunkHeight);
                
                for (int chunkY = 0; chunkY < visibleChunkHeight; chunkY++)
                {
                    for (int chunkX = 0; chunkX < visibleChunkWidth; chunkX++)
                    {
                        int currChunkX = Mathf.FloorToInt(_chunkWidth * (chunkX * chunkXT));
                        int currChunkY = Mathf.FloorToInt(_chunkHeight * (chunkY * chunkYT));

                        if (worldX + chunkX >= _width) break;
                        if (worldY + chunkY >= _height) return;

                        float[,] noise = PerlinNoise.GeneratePerlinNoise(1, 1, _seed, _scale, _octaves, offset + new Vector2(currChunkX, currChunkY), 0.5f, 2, 0, PerlinNoise.NormalizeMode.Global);

                        _heightMap[worldX + chunkX, worldY + chunkY] = noise[0, 0];
                    }
                }
            }
        }
    }

    private void GenerateTexture()
    {
        transform.localScale = new Vector2(_width, _height);
        Texture2D texture = new Texture2D(_width, _height);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, _width, _height), Vector2.zero);
        _sr.sprite = sprite;
        _heightMap = new float[_width, _height];
    }

    private void DrawTexture()
    {
        Color[] colors = new Color[_width * _height];

        // Fill black.
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }

        for (int y = 0, i = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++, i++)
            {
                if (_heightMap[x, y] < 0.5f)
                    colors[i] = Color.cyan;
                else {
                    colors[i] = Color.green;
                }
            }
        }

        _sr.sprite.texture.SetPixels(colors);
        _sr.sprite.texture.Apply();
    }
}