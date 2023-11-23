using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public Transform Goal { get; private set; }
    public Transform Player { get; private set; }
    public List<Transform> Boxes { get; private set; }
    public int BoardHeight { get; private set; }
    public int BoardWidth { get; private set; }
    public int BoxCount { get; private set; }

    private enum GoalWall
    {
        Bottom,
        Right,
        Top,
        Left
    }

    [SerializeField] private SlideTile slidePrefab;
    [SerializeField] private MountainTile mountainPrefab;
    [SerializeField] private GoalTile goalPrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject playerPrefab;

    private Vector3Int _startLocation;
    private Grid _grid;
    private Transform _mainCamera;
    private Transform _background;
    private readonly Dictionary<Vector3Int, Tile> _tiles = new();
    private readonly List<SlideTile> _slideTiles = new();
    private List<Vector3Int> _boxStartLocations;

    private readonly float _tileOffset = -0.6f;

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

        Boxes = new();
        _startLocation = new();
        _boxStartLocations = new();
        BoardWidth = SliderSettings.Values[0];
        BoardHeight = SliderSettings.Values[1];
        BoxCount = SliderSettings.Values[2];
        PrepareGameboard();
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

    public Tile GetTileAtPosition(Vector3Int position)
    {
        if (_tiles.TryGetValue(position, out var tile)) {
            return tile;
        }
        return null;
    }

    public Vector3Int GetClosestCell(Vector3 position)
    {
        Vector3Int closestCell = _grid.WorldToCell(position);
        return closestCell;
    }

    public void MoveUnit(Transform unit, Vector3Int target)
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

        // Move the camera over the center of the board and offset it slightly to place board in the desired location on screen.
        float max = Mathf.Max(BoardWidth, BoardHeight);
        Vector3 position = new(BoardWidth * 0.5f - 0.5f, max + 6, BoardHeight * 0.5f - 0.5f - (max * 0.125f));
        _mainCamera.SetPositionAndRotation(position, Quaternion.Euler(90, 0, 0));

        // Also set the background underneath the camera.
        Vector3 backgroundPos = position;
        backgroundPos.y = _tileOffset - 1;
        _background.position = backgroundPos;
    }

    public void ResetGameboard()
    {
        Player.position = _startLocation;
        for (int i = 0; i < _boxStartLocations.Count; i++) {
            Boxes[i].transform.position = _boxStartLocations[i];
        }
        UpdateTiles();
    }

    public void SetGameboard(Vector3Int player, List<Vector3Int> boxes)
    {
        Player.position = player;
        for (int i = 0; i < boxes.Count; i++) {
            Boxes[i].transform.position = boxes[i];
        }
        UpdateTiles();
    }

    public void UpdateTiles()
    {
        foreach (SlideTile tile in _slideTiles) {
            tile.BlocksMove = false;
        }
        foreach (Transform box in Boxes) {
            Tile nearestTile = GetTileAtPosition(GetClosestCell(box.position));
            if (nearestTile != null) {
                box.parent = nearestTile.transform;
                nearestTile.BlocksMove = true;
            }
        }
        if (Player != null) {
            Tile playerTile = GetTileAtPosition(GetClosestCell(Player.position));
            if (playerTile != null) {
                Player.parent = playerTile.transform;
            }
        }
    }

    private void GenerateBoxes()
    {
        for (int i = 0; i < BoxCount; i++) {
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

    private void GenerateGoalTile()
    {
        Vector3 goalPosition = new();
        Vector3 wallPosition = new();

        // Choose the goal location.

        GoalWall goalWall = (GoalWall)Random.Range(0, 4);
        switch (goalWall) {
            case GoalWall.Bottom:
                goalPosition.z = -1;
                goalPosition.x = Random.Range(1, BoardWidth - 1);
                wallPosition.z = -2;
                wallPosition.x = goalPosition.x;
                break;
            case GoalWall.Right:
                goalPosition.x = BoardWidth;
                goalPosition.z = Random.Range(1, BoardHeight - 1);
                wallPosition.x = BoardWidth + 1;
                wallPosition.z = goalPosition.z;
                break;
            case GoalWall.Top:
                goalPosition.z = BoardHeight;
                goalPosition.x = Random.Range(1, BoardWidth - 1);
                wallPosition.z = BoardHeight + 1;
                wallPosition.x = goalPosition.x;
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
        CreateTile(mountainPrefab, wallPosition);
    }

    private Transform CreateTile(Tile selectedTile, Vector3 position)
    {
        position.y = _tileOffset + selectedTile.transform.localScale.y / 2;

        Tile tile = Instantiate(selectedTile, position, Quaternion.identity);
        tile.name = $"Tile {position.x} {position.z}";
        tile.transform.parent = transform;
        _tiles.Add(GetClosestCell(position), tile);

        // If it's a slide tile, alternate the color to look like a checkerboard.
        if (tile.TryGetComponent<SlideTile>(out var slideScript)) {
            bool isOffset = (position.x + position.z) % 2 == 1;
            slideScript.InitializeColor(isOffset);
            _slideTiles.Add(slideScript);
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

    private void GeneratePlayer()
    {
        Tile[] openTiles = new Tile[_tiles.Count];
        Vector3 position = new();

        // Load into the array all tiles that are open. Boxes have already been placed and Slide Tiles updated.
        int i = 0;
        foreach (var tile in _tiles) {
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
        _startLocation = GetClosestCell(position);
        Player = Instantiate(playerPrefab, _startLocation, Quaternion.identity).transform;
        Player.parent = GetTileAtPosition(_startLocation).transform;
    }

    private void GenerateSlideTiles()
    {
        _slideTiles.Clear();
        for (int z = 0; z < BoardHeight; z++) {
            for (int x = 0; x < BoardWidth; x++) {
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
        for (int x = -1; x < BoardWidth + 1; x++) {
            tilePosition = new(x, 0, -1);
            CreateTile(mountainPrefab, tilePosition);
        }
        // Right wall.
        for (int z = 0; z < BoardHeight + 1; z++) {
            tilePosition = new(BoardWidth, 0, z);
            CreateTile(mountainPrefab, tilePosition);
        }
        // Top wall.
        for (int x = BoardWidth - 1; x > -2; x--) {
            tilePosition = new(x, 0, BoardHeight);
            CreateTile(mountainPrefab, tilePosition);
        }
        // Left wall.
        for (int z = BoardHeight - 1; z > -1; z--) {
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
