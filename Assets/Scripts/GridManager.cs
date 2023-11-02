using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Grid Grid => myGrid;
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject player;

    private Transform mainCamera;
    private Dictionary<Vector3, Tile> tiles = new();

    private Grid myGrid;
    private readonly float verticalOffset = -0.6f;

    private void Start()
    {
        mainCamera = GameObject.Find("Main Camera").transform;
        myGrid = GetComponent<Grid>();

        GenerateTiles();
        Instantiate(player, transform.position, Quaternion.identity);
    }

    private void GenerateTiles()
    {
        for (int z=0; z < height; z++) {
            for (int x=0; x < width; x++) {
                Vector3 tilePosition = new(x, verticalOffset, z);
                Tile spawnedTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                spawnedTile.name = $"Tile {x} {z}";

                bool isOffset = (x % 2 == 0 && z % 2 != 0) || (x % 2 != 0 && z % 2 == 0);
                spawnedTile.InitializeColor(isOffset);

                tiles.Add(tilePosition, spawnedTile);
            }
        }
        mainCamera.position = new Vector3((float)width / 2 - 0.5f, 10f, (float)height / 2 - 0.5f);
    }

    public Tile GetTileAtPosition(Vector3 position)
    {
        if(tiles.TryGetValue(position, out var tile)) {
            return tile;
        }
        return null;
    }
}
