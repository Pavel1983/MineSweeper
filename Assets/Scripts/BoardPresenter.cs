using System;
using System.Collections.Generic;
using FastMerger.Game.View;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class BoardPresenter
{
    private enum GameState
    {
        Playing,
        Won,
        Lost
    }

    public event Action<int> EventHudResetRequested;
    public event Action<int> EventFlaggedCountChanged;
    public event Action<float> EventElapsedTimeChanged;
    public event Action<bool, float> EventGameFinished;

    private readonly LevelConfig _levelConfig;
    private readonly BoardViewport _boardViewport;
    private readonly TileView _tileViewPrefab;
    private readonly Sprite _tileSprite;
    private readonly Transform _tilesRoot;
    private readonly float _tileSpacing;

    private Board _board;
    private BoardRevealHelper _boardRevealHelper;
    private GameState _gameState = GameState.Playing;
    private bool _isTimerRunning;
    private float _elapsedSeconds;
    private readonly List<TileView> _tileViews = new();

    public int MinesCount => _levelConfig.MinesCount;
    public int FlaggedCount => _board?.GetFlaggedCount() ?? 0;
    public float ElapsedSeconds => _elapsedSeconds;

    public BoardPresenter(
        LevelConfig levelConfig,
        BoardViewport boardViewport,
        TileView tileViewPrefab,
        Sprite tileSprite,
        Transform tilesRoot,
        float tileSpacing)
    {
        _levelConfig = levelConfig;
        _boardViewport = boardViewport;
        _tileViewPrefab = tileViewPrefab;
        _tileSprite = tileSprite;
        _tilesRoot = tilesRoot;
        _tileSpacing = tileSpacing;

        _boardViewport.BoundsChanged += RebuildTiles;
    }

    public void Dispose()
    {
        _boardViewport.BoundsChanged -= RebuildTiles;
        ClearTiles();
    }

    public void Tick()
    {
        UpdateTimer();
        HandleFlagInput();
    }

    public void RestartGame()
    {
        if (!TryInitBoard())
        {
            return;
        }

        _elapsedSeconds = 0f;
        _isTimerRunning = false;
        RebuildTiles();
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
        _gameState = GameState.Playing;
        return true;
    }

    private void HandleFlagInput()
    {
        if (_gameState != GameState.Playing
            || _board == null
            || Mouse.current == null
            || !Mouse.current.rightButton.wasPressedThisFrame)
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

    private void UpdateTimer()
    {
        if (!_isTimerRunning)
        {
            return;
        }

        _elapsedSeconds += Time.deltaTime;
        EventElapsedTimeChanged?.Invoke(_elapsedSeconds);
    }

    private void OnTileClicked(TileView tileView)
    {
        if (_gameState != GameState.Playing)
        {
            return;
        }

        var result = _boardRevealHelper.Reveal(tileView.Col, tileView.Row);
        if (!result.Success)
        {
            return;
        }

        NotifyFirstUserAction();
        ApplyRevealResult(result);

        if (result.HitMine)
        {
            _gameState = GameState.Lost;
            RevealRemainingMines(tileView.Col, tileView.Row);
            SetBoardInteractable(false);
            NotifyGameFinished(isWin: false);
            return;
        }

        TryHandleWin();
    }

    private void OnTileFlagClicked(TileView tileView)
    {
        if (_gameState != GameState.Playing || !_board.TryToggleFlag(tileView.Col, tileView.Row))
        {
            return;
        }

        NotifyFirstUserAction();
        tileView.SetFlagged(_board.IsFlagged(tileView.Col, tileView.Row));
        UpdateFlagView();
        TryHandleWin();
    }

    private void TryHandleWin()
    {
        if (!_board.IsWin())
        {
            return;
        }

        _gameState = GameState.Won;
        NotifyGameFinished(isWin: true);
    }

    private void NotifyGameFinished(bool isWin)
    {
        StopTimer();
        EventGameFinished?.Invoke(isWin, _elapsedSeconds);
    }

    private void NotifyFirstUserAction()
    {
        if (_isTimerRunning)
        {
            return;
        }

        _isTimerRunning = true;
    }

    private void StopTimer()
    {
        _isTimerRunning = false;
    }

    private void UpdateFlagView()
    {
        EventFlaggedCountChanged?.Invoke(_board.GetFlaggedCount());
    }

    private void ResetHud()
    {
        EventHudResetRequested?.Invoke(MinesCount);
    }

    private void ApplyRevealResult(RevealResult result)
    {
        foreach (var cell in result.OpenedCells)
        {
            var isTriggeredMine = result.HitMine && cell.IsMine;
            _tileViews[GetTileIndex(cell.Col, cell.Row)]
                .ShowRevealed(cell.NeighborMines, cell.IsMine, isTriggeredMine);
        }
    }

    private void RevealRemainingMines(int triggeredCol, int triggeredRow)
    {
        var cols = _levelConfig.Cols;
        var rows = _levelConfig.Rows;

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                if (!_board.IsMine(col, row) || (col == triggeredCol && row == triggeredRow))
                {
                    continue;
                }

                if (_board.IsRevealed(col, row))
                {
                    continue;
                }

                _board.TryRevealCell(col, row);
                _tileViews[GetTileIndex(col, row)].ShowRevealed(0, isMine: true);
            }
        }
    }

    private int GetTileIndex(int col, int row)
    {
        return row * _levelConfig.Cols + col;
    }

    private void SetBoardInteractable(bool interactable)
    {
        for (var i = 0; i < _tileViews.Count; i++)
        {
            _tileViews[i].SetInteractable(interactable);
        }
    }

    private void RebuildTiles()
    {
        ClearTiles();
        _gameState = GameState.Playing;
        ResetHud();

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

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                var position = origin + new Vector3(
                    col * pitch + cellSize * 0.5f,
                    row * pitch + cellSize * 0.5f,
                    0f
                );

                var tileView = Object.Instantiate(_tileViewPrefab, position, Quaternion.identity, _tilesRoot);
                if (!tileView.Init(col, row, cellSize, _tileSprite))
                {
                    Object.Destroy(tileView.gameObject);
                    continue;
                }

                tileView.EventClick += OnTileClicked;
                _tileViews.Add(tileView);
            }
        }

        SetBoardInteractable(true);
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

    private void ClearTiles()
    {
        for (var i = 0; i < _tileViews.Count; i++)
        {
            if (_tileViews[i] != null)
            {
                _tileViews[i].EventClick -= OnTileClicked;
                Object.Destroy(_tileViews[i].gameObject);
            }
        }

        _tileViews.Clear();
    }
}
