using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public Grid Grid { get; private set; }

    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject player;

    private Transform mainCamera;
    private Dictionary<Vector3, Tile> tiles = new();

    private readonly float verticalOffset = -0.6f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start()
    {
        mainCamera = GameObject.Find("Main Camera").transform;
        Grid = GetComponent<Grid>();

        GenerateTiles();
        Instantiate(player, transform.position, Quaternion.identity);
    }

    private void GenerateTiles()
    {
        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                //Create each tile and name it.
                Vector3 tilePosition = new(x, verticalOffset, z);
                Tile spawnedTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                spawnedTile.name = $"Tile {x} {z}";

                //Change the color of offset tiles to look like a checkerboard.
                bool isOffset = (x % 2 == 0 && z % 2 != 0) || (x % 2 != 0 && z % 2 == 0);
                spawnedTile.InitializeColor(isOffset);

                tiles.Add(tilePosition, spawnedTile);
            }
        }
        mainCamera.position = new Vector3((float)width / 2 - 0.5f, 10f, (float)height / 2 - 0.5f);
    }

    public Tile GetTileAtPosition(Vector3 position)
    {
        if (tiles.TryGetValue(new Vector3 (position.x, verticalOffset, position.z), out var tile)) {
            return tile;
        }
        return null;
    }

    public Vector3Int GetClosestCell(Vector3 position)
    {
        Vector3Int closestCell = Grid.WorldToCell(position);
        return closestCell;
    }

    public void MoveUnit(Transform unit, Vector3Int target, bool animate = true)
    {
        unit.position = target;
    }
}
