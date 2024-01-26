using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public Transform Goal { get; private set; }
    public Transform Player { get; private set; }
    public List<Transform> Boxes { get; private set; } = new();
    public int BoardHeight { get; private set; }
    public int BoardWidth { get; private set; }
    public int BoxCountSelection { get; private set; }
    public bool SearchIsRunning = false;

    private enum GoalWall
    {
        Bottom,
        Right,
        Top,
        Left
    }

    [SerializeField] private SlideTile slidePrefab;
    [SerializeField] private MountainTile wallTilePrefab;
    [SerializeField] private GoalTile goalPrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject playerPrefab;

    private Grid _grid;
    private Transform _mainCamera;
    private Transform _background;

    private Vector3Int _startLocation = new();
    private readonly Dictionary<Vector3Int, Tile> _tiles = new();
    private readonly List<Vector3Int> _boxStartLocations = new();

    private const float _tileOffset = -0.6f;

    // Singleton pattern.
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start()
    {
        _mainCamera = GameObject.Find("Main Camera").transform;
        _grid = GetComponent<Grid>();
        _background = GameObject.Find("Background").transform;

        // Read game parameters from the title screen.
        BoardWidth = SliderSettings.Values[0];
        BoardHeight = SliderSettings.Values[1];
        BoxCountSelection = SliderSettings.Values[2];

        GenerateGameboard();

        OrientCameraAndBackground();
    }

    private void GenerateGameboard()
    {
        // Time dependency: first Wall tiles, then the Goal tile.
        GenerateAndInsertWalls();
        GenerateAndInsertGoalTile();

        // Time dependency: first Slide Tiles, then Boxes, then the player.
        GenerateAndInsertSlideTiles();
        GenerateAndInsertBoxes();
        GenerateAndInsertPlayer();
    }

    private void OrientCameraAndBackground()
    {
        // The bigger the gameboard, the higher the camera is set.
        float largerDimension = Mathf.Max(BoardWidth, BoardHeight);
        Vector3 position = new(BoardWidth * 0.5f - 0.5f, largerDimension + 6, BoardHeight * 0.5f - 0.5f - (largerDimension * 0.125f));
        _mainCamera.SetPositionAndRotation(position, Quaternion.Euler(90, 0, 0));
        // TODO: instead of moving the camera farther back, scale the gameboard down.

        // Also set the background underneath the camera.
        Vector3 backgroundPosition = position;
        backgroundPosition.y = _tileOffset - 1;
        _background.position = backgroundPosition;
    }

    private void GenerateAndInsertWalls()
    {
        Vector3 currentTilePosition;

        // Create the bottom wall from left to right.
        for (var x = -1; x <= BoardWidth; x++) {
            currentTilePosition = new(x, 0, -1);
            CreateTile(wallTilePrefab, currentTilePosition);
        }
        // Create the right wall from bottom to top.
        for (var z = 0; z <= BoardHeight; z++) {
            currentTilePosition = new(BoardWidth, 0, z);
            CreateTile(wallTilePrefab, currentTilePosition);
        }
        // Create the top wall from right to left.
        for (var x = BoardWidth - 1; x >= -1; x--) {
            currentTilePosition = new(x, 0, BoardHeight);
            CreateTile(wallTilePrefab, currentTilePosition);
        }
        // Create the left wall from top to bottom.
        for (var z = BoardHeight - 1; z >= 0; z--) {
            currentTilePosition = new(-1, 0, z);
            CreateTile(wallTilePrefab, currentTilePosition);
        }
    }

    private void GenerateAndInsertGoalTile()
    {
        Vector3 goalPosition = new();
        Vector3 wallPosition = new();

        // Choose which wall the goal will be on.
        var goalWall = (GoalWall)Random.Range(0, 4);

        // The outermost positions on each wall are excluded as possible locations since they have trivial solutions.
        switch (goalWall)
        {
            case GoalWall.Bottom:
                goalPosition.x = Random.Range(1, BoardWidth - 1);
                goalPosition.z = -1;
                wallPosition.x = goalPosition.x;
                wallPosition.z = -2;
                break;

            case GoalWall.Right:
                goalPosition.x = BoardWidth;
                goalPosition.z = Random.Range(1, BoardHeight - 1);
                wallPosition.x = BoardWidth + 1;
                wallPosition.z = goalPosition.z;
                break;

            case GoalWall.Top:
                goalPosition.x = Random.Range(1, BoardWidth - 1);
                goalPosition.z = BoardHeight;
                wallPosition.x = goalPosition.x;
                wallPosition.z = BoardHeight + 1;
                break;

            case GoalWall.Left:
                goalPosition.x = -1;
                goalPosition.z = Random.Range(1, BoardHeight - 1);
                wallPosition.x = -2;
                wallPosition.z = goalPosition.z;
                break;
        }

        DestroyTile(goalPosition);
        Goal = CreateTile(goalPrefab, goalPosition);

        CreateTile(wallTilePrefab, wallPosition);
    }

    private Transform CreateTile(Tile selectedTile, Vector3 position)
    {
        position.y = _tileOffset + selectedTile.transform.localScale.y / 2;

        Tile tile = Instantiate(selectedTile, position, Quaternion.identity);
        tile.name = $"Tile {position.x} {position.z}";
        tile.transform.parent = transform;
        _tiles.Add(GetClosestCell(position), tile);

        // If it's a slide tile, alternate the color of offset tiles to look like a checkerboard.
        if (tile.TryGetComponent<SlideTile>(out var script)) {
            bool isOffset = ((position.x + position.z) % 2) == 1;
            script.InitializeColor(isOffset);
        }
        return tile.transform;
    }

    private void DestroyTile(Vector3 tilePosition)
    {
        Tile tile = GetTileAtPosition(GetClosestCell(tilePosition));
        if (tile != null) {
            _tiles.Remove(GetClosestCell(tilePosition));
            Destroy(tile.gameObject);
        }
    }

    public Vector3Int GetClosestCell(Vector3 position)
    {
        Vector3Int closestCell = _grid.WorldToCell(position);
        return closestCell;
    }

    public Tile GetTileAtPosition(Vector3Int position)
    {
        if (_tiles.TryGetValue(position, out var tile)) {
            return tile;
        }
        return null;
    }

    private void GenerateAndInsertSlideTiles()
    {
        for (int z = 0; z < BoardHeight; z++) {
            for (int x = 0; x < BoardWidth; x++) {
                Vector3 position = new(x, 0, z);
                CreateTile(slidePrefab, position);
            }
        }
    }
    private void GenerateAndInsertBoxes()
    {
        for (int i = 0; i < BoxCountSelection; i++) {
            int x = Random.Range(0, BoardWidth);
            int z = Random.Range(0, BoardHeight);
            Vector3Int newBoxPosition = new(x, 0, z);

            bool isNewLocation = true;
            foreach (Transform box in Boxes) {
                if (GetClosestCell(box.position) == newBoxPosition) {
                    isNewLocation = false;
                    Debug.Log("Box location is taken, so instantiation was skipped.");
                    break;
                }
            }
            if (isNewLocation) {
                Boxes.Add(Instantiate(boxPrefab, newBoxPosition, Quaternion.identity).transform);
                _boxStartLocations.Add(newBoxPosition);
            }
        }
        UpdateTiles();
    }

    public void UpdateTiles()
    {
        foreach (var tile in _tiles.Values.OfType<SlideTile>())
        {
            tile.BlocksMove = false;
        }

        foreach (Transform box in Boxes)
        { 
            Tile nearestTile = GetTileAtPosition(GetClosestCell(box.position));
            box.parent = nearestTile.transform;
            nearestTile.BlocksMove = true;
        }

        // Tiles are first updated before the player is instantiated.
        if (Player != null) {
            Tile playerTile = GetTileAtPosition(GetClosestCell(Player.position));
            Player.parent = playerTile.transform;
        }
    }

    private void GenerateAndInsertPlayer()
    {
        List<Tile> openTiles = new(_tiles.Count);

        // Some Slide tiles have boxes and are blocked.
        foreach (var tile in _tiles.Values.OfType<SlideTile>())
        {
            if (!tile.BlocksMove)
            {
                openTiles.Add(tile);
            }
        }

        Tile[] array = openTiles.ToArray();
        RandomizeWithFisherYates(array);

        _startLocation = GetClosestCell(array[0].transform.position);

        Player = Instantiate(playerPrefab, _startLocation, Quaternion.identity).transform;
        Player.parent = GetTileAtPosition(_startLocation).transform;
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

    public void MoveUnit(Transform unit, Vector3Int target)
    {
        unit.position = target;
    }

    public void SetGameboard(Vector3Int player, List<Vector3Int> boxes)
    {
        Player.position = player;
        for (int i = 0; i < boxes.Count; i++) {
            Boxes[i].transform.position = boxes[i];
        }
        UpdateTiles();
    }

    public void RequestReset()
    {
        ResetGameboard();
    }

    private void ResetGameboard()
    {
        Player.position = _startLocation;
        for (int i = 0; i < _boxStartLocations.Count; i++) {
            Boxes[i].transform.position = _boxStartLocations[i];
        }
        UpdateTiles();
    }

    public void RequestNewGameboard()
    {
        GenerateGameboard();
    }

    public void DestroyGameboard()
    {
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
        foreach (Transform box in Boxes) {
            Destroy(box.gameObject);
        }
        Destroy(Player.gameObject);

        _tiles.Clear();
        Boxes.Clear();
        _boxStartLocations.Clear();
    }
}
