using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public Grid Grid { get; private set; }
    public List<GameObject> boxes = new();

    public int boardHeight, boardWidth, boxCount;
    [SerializeField] private SlideTile slidePrefab;
    [SerializeField] private MountainTile mountainPrefab;
    [SerializeField] private GoalTile goalPrefab;
    [SerializeField] private GameObject boxPrefab;
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
    }

    private void Start()
    {
        mainCamera = GameObject.Find("Main Camera").transform;
        Grid = GetComponent<Grid>();

        PrepareGameboard();
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

    public void UpdateTiles()
    {
        foreach (var tile in tiles) {
            if (tile.Value.TryGetComponent(out SlideTile slideTile)) {
                slideTile.BlocksMove = false;
            }
        }
        foreach (GameObject box in boxes) {
            Tile boxTile = GetTileAtPosition(GetClosestCell(box.transform.position));
            if (boxTile != null) {
                boxTile.BlocksMove = true;
            }

        }
    }

    private void GeneratePlayer()
    {
        Tile[] openTiles = new Tile[tiles.Count];
        Vector3 position = new();

        // Load into the array all tiles that are open. Boxes have already been placed and Slide Tiles updated.
        int i = 0;
        foreach (var tile in tiles) {
            if (tile.Value.TryGetComponent<Tile>(out var tileScript)) {
                if (!tileScript.BlocksMove) {
                    openTiles[i] = tileScript;
                }
            }
            i++;
        }

        RandomizeWithFisherYates(openTiles);

        foreach (Tile tile in openTiles) {
            if (tile != null) {
                position = tile.transform.position;
                break;
            }
        }
        Instantiate(player, position, Quaternion.identity);
    }

    private void GenerateBoxes()
    {
        for (int i = 0; i < boxCount; i++) {
            int x = Random.Range(0, boardWidth);
            int z = Random.Range(0, boardHeight);
            Vector3Int newBoxPosition = new(x, 0, z);

            bool isNewLocation = true;
            foreach (GameObject box in boxes) {
                if (box.transform.position == newBoxPosition) {
                    isNewLocation = false;
                    Debug.Log("Box location is taken, so instantiation was skipped.");
                    break;
                }
            }
            if (isNewLocation) {
                boxes.Add(Instantiate(boxPrefab, newBoxPosition, Quaternion.identity));
            }
        }
        UpdateTiles();
    }

    private void GenerateGoalTile()
    {
        // Select the goal tile.
        Vector3 goalPosition = new();
        Vector3 wallPosition = new();
        GoalQuadrant goalQuadrant = (GoalQuadrant)Random.Range(0, 4);
        switch (goalQuadrant) {
            case GoalQuadrant.Bottom:
                goalPosition.z = -1;
                goalPosition.x = Random.Range(0, boardWidth);
                wallPosition.z = -2;
                wallPosition.x = goalPosition.x;
                break;
            case GoalQuadrant.Right:
                goalPosition.x = boardWidth;
                goalPosition.z = Random.Range(0, boardHeight);
                wallPosition.x = boardWidth + 1;
                wallPosition.z = goalPosition.z;
                break;
            case GoalQuadrant.Top:
                goalPosition.z = boardHeight;
                goalPosition.x = Random.Range(0, boardWidth);
                wallPosition.z = boardHeight + 1;
                wallPosition.x = goalPosition.x;
                break;
            case GoalQuadrant.Left:
                goalPosition.x = -1;
                goalPosition.z = Random.Range(0, boardHeight);
                wallPosition.x = -2;
                wallPosition.z = goalPosition.z;
                break;
        }

        // Remove the wall tile and place a goal tile at the selected position.
        tiles.Remove(GetClosestCell(goalPosition));
        GameObject wall = transform.Find($"Tile {goalPosition.x} {goalPosition.z}").gameObject;
        Destroy(wall);

        goalPosition.y = tileOffset + goalPrefab.transform.localScale.y / 2;
        Tile goalTile = Instantiate(goalPrefab, goalPosition, Quaternion.identity);
        goalTile.name = $"Tile {goalPosition.x} {goalPosition.z}";
        goalTile.transform.parent = transform;

        wallPosition.y = tileOffset + mountainPrefab.transform.localScale.y / 2;
        Tile wallTile = Instantiate(mountainPrefab, wallPosition, Quaternion.identity);
        wallTile.name = $"Tile {wallPosition.x} {wallPosition.z}";
        wallTile.transform.parent = transform;

        tiles.Add(GetClosestCell(goalPosition), goalTile);
        tiles.Add(GetClosestCell(wallPosition), wallTile);
    }

    private void GenerateSlideTiles()
    {
        for (int z = 0; z < boardHeight; z++) {
            for (int x = 0; x < boardWidth; x++) {
                //Create each tile and name it.
                Vector3 tilePosition = new(x, tileOffset + slidePrefab.transform.localScale.y / 2, z);
                SlideTile slideTile = Instantiate(slidePrefab, tilePosition, Quaternion.identity);
                slideTile.name = $"Tile {x} {z}";
                slideTile.transform.parent = transform;

                //Change the color of offset tiles to look like a checkerboard.
                bool isOffset = (x + z) % 2 == 1;
                slideTile.InitializeColor(isOffset);

                tiles.Add(GetClosestCell(tilePosition), slideTile);
            }
        }
        UpdateTiles();
    }

    private void GenerateWalls()
    {
        Vector3 tilePosition;
        Tile spawnedTile;

        // Bottom wall.
        for (int x = -1; x < boardWidth + 1; x++) {
            tilePosition = new(x, tileOffset + slidePrefab.transform.localScale.y / 2, -1);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile {x} -1";
            spawnedTile.transform.parent = transform;

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
        // Right wall.
        for (int z = 0; z < boardHeight + 1; z++) {
            tilePosition = new(boardWidth, tileOffset + slidePrefab.transform.localScale.y / 2, z);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile {boardWidth} {z}";
            spawnedTile.transform.parent = transform;

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
        // Top wall.
        for (int x = boardWidth - 1; x > -2; x--) {
            tilePosition = new(x, tileOffset + slidePrefab.transform.localScale.y / 2, boardHeight);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile {x} {boardHeight}";
            spawnedTile.transform.parent = transform;

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
        // Left wall.
        for (int z = boardHeight - 1; z > -1; z--) {
            tilePosition = new(-1, tileOffset + slidePrefab.transform.localScale.y / 2, z);
            spawnedTile = Instantiate(mountainPrefab, tilePosition, Quaternion.identity);
            spawnedTile.name = $"Tile -1 {z}";
            spawnedTile.transform.parent = transform;

            tiles.Add(GetClosestCell(tilePosition), spawnedTile);
        }
    }

    private void PrepareGameboard()
    {
        GenerateSlideTiles();
        GenerateWalls();
        GenerateGoalTile();
        GenerateBoxes();
        GeneratePlayer();
        // Move the camera over the center of the board.
        mainCamera.position = new Vector3((float)boardWidth / 2 - 0.5f, 15f, (float)boardHeight / 2 - 0.5f);
    }

        private Tile[] RandomizeWithFisherYates(Tile[] array)
    {
        int count = array.Length;

        while (count > 1) {
            int i = Random.Range(0, count--);
            (array[i], array[count]) = (array[count], array[i]);
        }
        return array;
    }
}
