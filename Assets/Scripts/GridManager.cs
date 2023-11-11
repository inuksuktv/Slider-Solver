using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public Grid Grid { get; private set; }
    public Transform Goal { get; private set; }
    public Transform Player { get; private set; }
    public List<Transform> boxes = new();
    public Vector3Int startLocation = new();
    public List<Vector3Int> boxLocations = new();

    private enum GoalWall
    {
        Bottom,
        Right,
        Top,
        Left
    }

    public int boardHeight, boardWidth, boxCount;
    [SerializeField] private SlideTile slidePrefab;
    [SerializeField] private MountainTile mountainPrefab;
    [SerializeField] private GoalTile goalPrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject playerPrefab;

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

    public void DestroyGameboard()
    {
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
        foreach (Transform box in boxes) {
            Destroy(box.gameObject);
        }
        Destroy(Player.gameObject);
        tiles.Clear();
        boxes.Clear();
        boxLocations.Clear();
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

    public void PrepareGameboard()
    {
        GenerateSlideTiles();
        GenerateWalls();
        GenerateGoalTile();
        GenerateBoxes();
        GeneratePlayer();
        // Move the camera over the center of the board.
        mainCamera.position = new Vector3((float)boardWidth / 2 - 0.5f, 15f, (float)boardHeight / 2 - 0.5f);
    }

    public void UpdateTiles()
    {
        foreach (var tile in tiles) {
            if (tile.Value.TryGetComponent(out SlideTile slideTile)) {
                slideTile.BlocksMove = false;
            }
        }
        foreach (Transform box in boxes) {
            Tile boxTile = GetTileAtPosition(GetClosestCell(box.position));
            if (boxTile != null) {
                boxTile.BlocksMove = true;
            }

        }
    }

    private void GenerateBoxes()
    {
        for (int i = 0; i < boxCount; i++) {
            int x = Random.Range(0, boardWidth);
            int z = Random.Range(0, boardHeight);
            Vector3Int newBoxPosition = new(x, 0, z);

            bool isNewLocation = true;
            foreach (Transform box in boxes) {
                if (GetClosestCell(box.position) == newBoxPosition) {
                    isNewLocation = false;
                    Debug.Log("Box location is taken, so instantiation was skipped.");
                    break;
                }
            }
            if (isNewLocation) {
                boxes.Add(Instantiate(boxPrefab, newBoxPosition, Quaternion.identity).transform);
                boxLocations.Add(newBoxPosition);
            }
        }
        UpdateTiles();
    }

    private void GenerateGoalTile()
    {
        Vector3 goalPosition = new();
        Vector3 wallPosition = new();

        // Choose the goal location.

        GoalWall goalWall = (GoalWall)Random.Range(0, 4);
        switch (goalWall) {
            case GoalWall.Bottom:
                goalPosition.z = -1;
                goalPosition.x = Random.Range(1, boardWidth - 1);
                wallPosition.z = -2;
                wallPosition.x = goalPosition.x;
                break;
            case GoalWall.Right:
                goalPosition.x = boardWidth;
                goalPosition.z = Random.Range(1, boardHeight - 1);
                wallPosition.x = boardWidth + 1;
                wallPosition.z = goalPosition.z;
                break;
            case GoalWall.Top:
                goalPosition.z = boardHeight;
                goalPosition.x = Random.Range(1, boardWidth - 1);
                wallPosition.z = boardHeight + 1;
                wallPosition.x = goalPosition.x;
                break;
            case GoalWall.Left:
                goalPosition.x = -1;
                goalPosition.z = Random.Range(1, boardHeight - 1);
                wallPosition.x = -2;
                wallPosition.z = goalPosition.z;
                break;
        }

        DestroyTile(goalPosition);
        Goal = CreateTile(goalPrefab, goalPosition);
        CreateTile(mountainPrefab, wallPosition);
    }

    private Transform CreateTile(Tile selectedTile, Vector3 position)
    {
        position.y = tileOffset + selectedTile.transform.localScale.y / 2;

        Tile tile = Instantiate(selectedTile, position, Quaternion.identity);
        tile.name = $"Tile {position.x} {position.z}";
        tile.transform.parent = transform;
        tiles.Add(GetClosestCell(position), tile);

        // If it's a slide tile, alternate the color to look like a checkerboard.
        if (tile.TryGetComponent<SlideTile>(out var slideScript)) {
            bool isOffset = (position.x + position.z) % 2 == 1;
            slideScript.InitializeColor(isOffset);
        }
        return tile.transform;
    }

    private void DestroyTile(Vector3 tilePosition)
    {
        Tile tile = GetTileAtPosition(GetClosestCell(tilePosition));
        if (tile != null) {
            tiles.Remove(GetClosestCell(tilePosition));
            Destroy(tile.gameObject);
        }
    }

    private void GeneratePlayer()
    {
        Tile[] openTiles = new Tile[tiles.Count];
        Vector3 position = new();

        // Load into the array all tiles that are open. Boxes have already been placed and Slide Tiles updated.
        int i = 0;
        foreach (var tile in tiles) {
            Tile script = tile.Value;
            if (script.gameObject.CompareTag("Slide") && !tile.Value.BlocksMove) {
                openTiles[i] = tile.Value;
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
        startLocation = GetClosestCell(position);
        Player = Instantiate(playerPrefab, startLocation, Quaternion.identity).transform;
    }

    private void GenerateSlideTiles()
    {
        for (int z = 0; z < boardHeight; z++) {
            for (int x = 0; x < boardWidth; x++) {
                Vector3 position = new(x, 0, z);
                CreateTile(slidePrefab, position);
            }
        }
        UpdateTiles();
    }

    private void GenerateWalls()
    {
        Vector3 tilePosition;

        // Bottom wall.
        for (int x = -1; x < boardWidth + 1; x++) {
            tilePosition = new(x, 0, -1);
            CreateTile(mountainPrefab, tilePosition);
        }
        // Right wall.
        for (int z = 0; z < boardHeight + 1; z++) {
            tilePosition = new(boardWidth, 0, z);
            CreateTile(mountainPrefab, tilePosition);
        }
        // Top wall.
        for (int x = boardWidth - 1; x > -2; x--) {
            tilePosition = new(x, 0, boardHeight);
            CreateTile(mountainPrefab, tilePosition);
        }
        // Left wall.
        for (int z = boardHeight - 1; z > -1; z--) {
            tilePosition = new(-1, 0, z);
            CreateTile(mountainPrefab, tilePosition);
        }
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
