using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public Grid Grid { get; private set; }

    [SerializeField] private int width, height;
    [SerializeField] private SlideTile slidePrefab;
    [SerializeField] private MountainTile mountainPrefab;
    [SerializeField] private GameObject player;

    private Transform mainCamera;
    private Dictionary<Vector3Int, Tile> tiles = new();

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
        // Generate the playable tiles.
        Vector3 tilePosition;
        SlideTile slideTile;
        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                //Create each tile and name it.
                tilePosition = new(x, verticalOffset, z);
                slideTile = Instantiate(slidePrefab, tilePosition, Quaternion.identity);
                slideTile.name = $"Tile {x} {z}";

                //Change the color of offset tiles to look like a checkerboard.
                bool isOffset = (x + z) % 2 == 1;
                slideTile.InitializeColor(isOffset);

                tiles.Add(GetClosestCell(tilePosition), slideTile);
            }
        }

        Tile spawnedTile;
        // Generate the wall tiles.
        for (int x = -1; x < width + 1; x++) {
            tilePosition = new(x, verticalOffset + 0.5f, -1);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile {x} -1";

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
        for (int z = 0; z < height + 1; z++) {
            tilePosition = new(width, verticalOffset + 0.5f, z);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile {width} {z}";

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
        for (int x = width - 1; x > -2; x--) {
            tilePosition = new(x, verticalOffset + 0.5f, height);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile {x} {height}";

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
        for (int z = height - 1; z > -1; z--) {
            tilePosition = new(-1, verticalOffset + 0.5f, z);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile -1 {z}";

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }

        // Move the camera over the center of the board.
        mainCamera.position = new Vector3((float)width / 2 - 0.5f, 10f, (float)height / 2 - 0.5f);
    }

    public Tile GetTileAtPosition(Vector3Int position)
    {
        if (tiles.TryGetValue(position, out var tile)) {
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
