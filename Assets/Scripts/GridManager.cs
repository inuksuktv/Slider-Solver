using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public Grid Grid { get; private set; }
    public List<GameObject> boxes = new();

    public int width, height, boxCount;
    [SerializeField] private SlideTile slidePrefab;
    [SerializeField] private MountainTile mountainPrefab;
    [SerializeField] private GoalTile goalPrefab;
    [SerializeField] private GameObject player;

    private enum GoalQuadrant
    {
        Bottom,
        Right,
        Top,
        Left
    }

    private Transform mainCamera;
    private Dictionary<Vector3Int, Tile> tiles = new();

    private readonly float tileOffset = -0.6f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }

        mainCamera = GameObject.Find("Main Camera").transform;
        Grid = GetComponent<Grid>();

        GenerateTiles();
        Instantiate(player);
    }

    private void Start()
    {
        
    }

    private void GenerateTiles()
    {
        // Generate the playable tiles.
        Vector3 tilePosition;
        SlideTile slideTile;
        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                //Create each tile and name it.
                tilePosition = new(x, tileOffset + 0.5f*slidePrefab.transform.localScale.y, z);
                slideTile = Instantiate(slidePrefab, tilePosition, Quaternion.identity);
                slideTile.name = $"Tile {x} {z}";
                slideTile.transform.parent = transform;

                //Change the color of offset tiles to look like a checkerboard.
                bool isOffset = (x + z) % 2 == 1;
                slideTile.InitializeColor(isOffset);

                tiles.Add(GetClosestCell(tilePosition), slideTile);
            }
        }

        // Generate the wall tiles.
        Tile spawnedTile;
        // Bottom wall.
        for (int x = -1; x < width + 1; x++) {
            tilePosition = new (x, tileOffset + 0.5f * slidePrefab.transform.localScale.y, -1);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile {x} -1";
            spawnedTile.transform.parent = transform;

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
        // Right wall.
        for (int z = 0; z < height + 1; z++) {
            tilePosition = new (width, tileOffset + 0.5f * slidePrefab.transform.localScale.y, z);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile {width} {z}";
            spawnedTile.transform.parent = transform;

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
        // Top wall.
        for (int x = width - 1; x > -2; x--) {
            tilePosition = new (x, tileOffset + 0.5f * slidePrefab.transform.localScale.y, height);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile {x} {height}";
            spawnedTile.transform.parent = transform;

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
        // Left wall.
        for (int z = height - 1; z > -1; z--) {
            tilePosition = new (-1, tileOffset + 0.5f * slidePrefab.transform.localScale.y, z);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile -1 {z}";
            spawnedTile.transform.parent = transform;

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }

        // Select the goal tile.
        tilePosition = new();
        GoalQuadrant goalQuadrant = (GoalQuadrant)Random.Range(0, 4);
        switch (goalQuadrant) {
            case GoalQuadrant.Bottom:
                tilePosition.z = -1;
                tilePosition.x = Random.Range(0, width);
                break;
            case GoalQuadrant.Right:
                tilePosition.x = width;
                tilePosition.z = Random.Range(0, height);
                break;
            case GoalQuadrant.Top:
                tilePosition.z = height;
                tilePosition.x = Random.Range(0, width);
                break;
            case GoalQuadrant.Left:
                tilePosition.x = -1;
                tilePosition.z = Random.Range(0, height);
                break;
        }
        // Remove the wall tile and place a goal tile at the selected position.
        tiles.Remove(GetClosestCell(tilePosition));
        GameObject wall = transform.Find($"Tile {tilePosition.x} {tilePosition.z}").gameObject;
        Destroy(wall);
        tilePosition.y = tileOffset + 0.5f * goalPrefab.transform.localScale.y;
        Tile goalTile = Instantiate(goalPrefab, tilePosition, Quaternion.identity);
        goalTile.name = $"Tile {tilePosition.x} {tilePosition.z}";
        goalTile.transform.parent = transform;

        tiles.Add(GetClosestCell(tilePosition), goalTile);

        // Move the camera over the center of the board.
        mainCamera.position = new Vector3((float)width / 2 - 0.5f, 15f, (float)height / 2 - 0.5f);
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
