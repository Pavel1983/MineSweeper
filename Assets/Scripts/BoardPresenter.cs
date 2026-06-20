using System.Collections.Generic;
using FastMerger.Game.View;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardPresenter : MonoBehaviour
{
    [SerializeField] private LevelConfig _levelConfig;
    [SerializeField] private BoardViewport _boardViewport;
    [SerializeField] private TileView _tileViewPrefab;
    [SerializeField] private Sprite _tileSprite;
    [SerializeField] private Transform _tilesRoot;
    [SerializeField, Min(0f)] private float _tileSpacing;

    private Board _board;
    private BoardRevealHelper _boardRevealHelper;
    private Transform _tilesContainer;
    private readonly List<TileView> _tileViews = new();

    private void Start()
    {
        if (!TryValidateSetup() || !TryInitBoard())
        {
            return;
        }

        _boardViewport.BoundsChanged += RebuildTiles;
        RebuildTiles();
    }

    private bool TryValidateSetup()
    {
        if (_levelConfig == null)
        {
            Debug.LogError("LevelConfig is not set up.");
            return false;
        }

        if (_boardViewport == null)
        {
            Debug.LogError("BoardViewport is not set up.");
            return false;
        }

        if (_tileViewPrefab == null)
        {
            Debug.LogError("TileView prefab is not set up.");
            return false;
        }

        if (_tileSprite == null)
        {
            Debug.LogError("Tile sprite is not set up.");
            return false;
        }

        return true;
    }

    private bool TryInitBoard()
    {
        _board = new Board();
        if (!_board.TryInit(_levelConfig.Cols, _levelConfig.Rows, _levelConfig.MinesCount))
        {
            Debug.LogError("Can't initialize the board.");
            return false;
        }

        _boardRevealHelper = new BoardRevealHelper(_board);
        return true;
    }

    private void Update()
    {
        if (_board == null || Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame)
        {
            return;
        }

        var camera = _boardViewport.TargetCamera;
        if (camera == null)
        {
            Debug.LogError("BoardPresenter: camera is not set up.");
            return;
        }

        var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider == null || !hit.collider.TryGetComponent(out TileView tileView))
        {
            return;
        }

        OnTileFlagClicked(tileView);
    }

    private void OnTileClicked(TileView tileView)
    {
        var result = _boardRevealHelper.Reveal(tileView.Col, tileView.Row);
        if (!result.Success)
        {
            return;
        }

        ApplyRevealResult(result);
    }

    private void OnTileFlagClicked(TileView tileView)
    {
        if (!_board.TryToggleFlag(tileView.Col, tileView.Row))
        {
            return;
        }

        tileView.SetFlagged(_board.IsFlagged(tileView.Col, tileView.Row));
    }

    private void ApplyRevealResult(RevealResult result)
    {
        foreach (var cell in result.OpenedCells)
        {
            _tileViews[GetTileIndex(cell.Col, cell.Row)]
                .ShowRevealed(cell.NeighborMines, cell.IsMine);
        }
    }

    private int GetTileIndex(int col, int row)
    {
        return row * _levelConfig.Cols + col;
    }

    private void OnDestroy()
    {
        if (_boardViewport != null)
        {
            _boardViewport.BoundsChanged -= RebuildTiles;
        }
    }

    private void RebuildTiles()
    {
        ClearTiles();

        var cols = _levelConfig.Cols;
        var rows = _levelConfig.Rows;

        if (!TryGetViewportBounds(cols, rows, out var bounds))
        {
            return;
        }

        if (!TryGetGridLayout(cols, rows, bounds, out var cellSize, out var origin))
        {
            return;
        }

        var pitch = cellSize + _tileSpacing;
        EnsureTilesContainer();

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                var position = origin + new Vector3(
                    col * pitch + cellSize * 0.5f,
                    row * pitch + cellSize * 0.5f,
                    0f
                );

                var tileView = Instantiate(_tileViewPrefab, position, Quaternion.identity, _tilesContainer);
                if (!tileView.Init(col, row, cellSize, _tileSprite))
                {
                    Destroy(tileView.gameObject);
                    continue;
                }

                tileView.EventClick += OnTileClicked;
                _tileViews.Add(tileView);
            }
        }
    }

    private bool TryGetViewportBounds(int cols, int rows, out Bounds bounds)
    {
        if (!_boardViewport.TryGetWorldBounds(cols, rows, out bounds))
        {
            Debug.LogError("Can't resolve board viewport bounds.");
            return false;
        }

        return true;
    }

    private bool TryGetGridLayout(int cols, int rows, Bounds bounds, out float cellSize, out Vector3 origin)
    {
        var spacing = _tileSpacing;
        var cellSizeX = (bounds.size.x - (cols - 1) * spacing) / cols;
        var cellSizeY = (bounds.size.y - (rows - 1) * spacing) / rows;
        cellSize = Mathf.Min(cellSizeX, cellSizeY);

        if (cellSize <= 0f)
        {
            Debug.LogError("Tile spacing is too large for the board viewport.");
            cellSize = 0f;
            origin = default;
            return false;
        }

        var gridWidth = cols * cellSize + (cols - 1) * spacing;
        var gridHeight = rows * cellSize + (rows - 1) * spacing;
        origin = bounds.center - new Vector3(gridWidth * 0.5f, gridHeight * 0.5f, 0f);
        return true;
    }

    private void EnsureTilesContainer()
    {
        if (_tilesRoot != null)
        {
            _tilesContainer = _tilesRoot;
            return;
        }

        if (_tilesContainer == null)
        {
            var containerObject = new GameObject("Tiles");
            containerObject.transform.SetParent(transform, worldPositionStays: false);
            containerObject.transform.localPosition = Vector3.zero;
            _tilesContainer = containerObject.transform;
        }
    }

    private void ClearTiles()
    {
        for (var i = 0; i < _tileViews.Count; i++)
        {
            if (_tileViews[i] != null)
            {
                _tileViews[i].EventClick -= OnTileClicked;
                Destroy(_tileViews[i].gameObject);
            }
        }

        _tileViews.Clear();
    }
}
