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
    public bool SearchIsRunning { get; set; } = false;
    public float TileOffset = -0.6f;

    [SerializeField] private SlideTile slidePrefab;
    [SerializeField] private WallTile wallTilePrefab;
    [SerializeField] private GoalTile goalPrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject playerPrefab;

    private enum GoalWall
    {
        Bottom,
        Right,
        Top,
        Left
    }

    private Grid _grid;
    private Vector3Int _startLocation = new();
    private readonly Dictionary<Vector3Int, Tile> _tiles = new();
    private readonly List<Vector3Int> _boxStartLocations = new();

    private void Awake()
    {
        // Singleton pattern.
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }

        // Read game parameters from the title screen.
        BoardWidth = SliderSettings.Values[0];
        BoardHeight = SliderSettings.Values[1];
        BoxCountSelection = SliderSettings.Values[2];
    }

    private void Start()
    {
        _grid = GetComponent<Grid>();

        GenerateGameboard();
    }

    public void GenerateGameboard()
    {
        // Time dependency: first Wall tiles, then the Goal tile.
        GenerateWalls();
        GenerateGoalTile();

        // Time dependency: first Slide Tiles, then Boxes, then the player.
        GenerateSlideTiles();
        GenerateBoxes();
        GeneratePlayer();
    }

    private void GenerateWalls()
    {
        Vector3 currentTilePosition;

        // Create the bottom wall from left to right.
        for (var x = -1; x <= BoardWidth; x++)
        {
            currentTilePosition = new(x, 0, -1);
            CreateTile(wallTilePrefab, currentTilePosition);
        }
        // Create the right wall from bottom to top.
        for (var z = 0; z <= BoardHeight; z++)
        {
            currentTilePosition = new(BoardWidth, 0, z);
            CreateTile(wallTilePrefab, currentTilePosition);
        }
        // Create the top wall from right to left.
        for (var x = (BoardWidth - 1); x >= -1; x--)
        {
            currentTilePosition = new(x, 0, BoardHeight);
            CreateTile(wallTilePrefab, currentTilePosition);
        }
        // Create the left wall from top to bottom.
        for (var z = (BoardHeight - 1); z >= 0; z--)
        {
            currentTilePosition = new(-1, 0, z);
            CreateTile(wallTilePrefab, currentTilePosition);
        }
    }

    private void GenerateGoalTile()
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
        position.y = TileOffset + (0.5f * selectedTile.transform.localScale.y);

        var tile = Instantiate(selectedTile, position, Quaternion.identity);
        tile.name = $"Tile {position.x} {position.z}";
        tile.transform.parent = transform;
        _tiles.Add(GetClosestCell(position), tile);

        // If it's a slide tile, vary the color of offset tiles to look like a checkerboard.
        if (tile.TryGetComponent<SlideTile>(out var script))
        {
            bool isOffset = ((position.x + position.z) % 2) == 1;
            script.InitializeColor(isOffset);
        }
        return tile.transform;
    }

    private void DestroyTile(Vector3 tilePosition)
    {
        Tile tile = GetTileAtPosition(GetClosestCell(tilePosition));
        if (tile != null)
        {
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
        if (_tiles.TryGetValue(position, out var tile))
        {
            return tile;
        }
        return null;
    }

    private void GenerateSlideTiles()
    {
        for (int z = 0; z < BoardHeight; z++)
        {
            for (int x = 0; x < BoardWidth; x++)
            {
                Vector3 position = new(x, 0, z);
                CreateTile(slidePrefab, position);
            }
        }
    }

    private void GenerateBoxes()
    {
        for (int i = 0; i < BoxCountSelection; i++)
        {
            int x = Random.Range(0, BoardWidth);
            int z = Random.Range(0, BoardHeight);
            Vector3Int newBoxPosition = new(x, 0, z);

            bool isNewLocation = true;
            foreach (Transform box in Boxes)
            {
                if (GetClosestCell(box.position) == newBoxPosition)
                {
                    isNewLocation = false;
                    Debug.Log("Box location is taken, so instantiation was skipped.");
                    break;
                }
            }
            if (isNewLocation)
            {
                Boxes.Add(Instantiate(boxPrefab, newBoxPosition, Quaternion.identity).transform);
                _boxStartLocations.Add(newBoxPosition);
            }
        }
        UpdateTiles();
    }

    private void GeneratePlayer()
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

        while (count > 1)
        {
            int i = Random.Range(0, count--);
            (array[i], array[count]) = (array[count], array[i]);
        }
        return array;
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

        // Player hasn't been instantiated yet the first time this method is called.
        if (Player != null)
        {
            Tile playerTile = GetTileAtPosition(GetClosestCell(Player.position));
            Player.parent = playerTile.transform;
        }
    }

    public void DestroyGameboard()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        foreach (Transform box in Boxes)
        {
            Destroy(box.gameObject);
        }
        Destroy(Player.gameObject);

        _tiles.Clear();
        Boxes.Clear();
        _boxStartLocations.Clear();
    }

    public void MoveUnit(Transform unit, Vector3Int target)
    {
        unit.position = target;
    }

    public void ResetGameboard()
    {
        Player.position = _startLocation;
        for (int i = 0; i < _boxStartLocations.Count; i++)
        {
            Boxes[i].transform.position = _boxStartLocations[i];
        }
        UpdateTiles();
    }

    public void SetGameboard(Vertex.GameState state)
    {
        Player.position = state.PlayerLocation;
        for (int i = 0; i < state.BoxLocations.Count; i++)
        {
            Boxes[i].transform.position = state.BoxLocations[i];
        }
        UpdateTiles();
    }
}
