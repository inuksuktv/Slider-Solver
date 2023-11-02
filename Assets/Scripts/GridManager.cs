using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _tilePrefab;

    [SerializeField] private Transform _cam;

    private Dictionary<Vector3, Tile> _tiles = new Dictionary<Vector3, Tile>();

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        for (int z=0; z < _width; z++) {
            for (int x=0; x < _height; x++) {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, -0.6f, z), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {z}";

                var isOffset = (x % 2 == 0 && z % 2 != 0) || (x % 2 != 0 && z % 2 == 0);
                spawnedTile.Init(isOffset);

                _tiles.Add(new Vector3(x, -0.6f, z), spawnedTile);
            }
        }
        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, 10, (float)_height / 2 - 0.5f);
    }

    public Tile GetTileAtPosition(Vector3 pos)
    {
        if(_tiles.TryGetValue(pos, out var tile)) {
            return tile;
        }
        return null;
    }
}
