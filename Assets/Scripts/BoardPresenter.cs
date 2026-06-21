using System;
using System.Collections.Generic;
using FastMerger.Game.View;
using UnityEngine;
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
    private readonly GameTimer _gameTimer;
    private readonly BoardInputHandler _inputHandler;

    private Board _board;
    private BoardRevealHelper _boardRevealHelper;
    private GameState _gameState = GameState.Playing;
    private bool _isFirstReveal = true;
    private readonly List<TileView> _tileViews = new();

    public int MinesCount => _levelConfig.MinesCount;
    public int FlaggedCount => _board?.GetFlaggedCount() ?? 0;
    public float ElapsedSeconds => _gameTimer.ElapsedSeconds;

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

        _gameTimer = new GameTimer();
        _gameTimer.ElapsedTimeChanged += OnElapsedTimeChanged;

        _inputHandler = new BoardInputHandler(boardViewport);
        _inputHandler.EventTileFlagClicked += OnTileFlagClicked;

        _boardViewport.BoundsChanged += RebuildTiles;
    }

    public void Dispose()
    {
        _boardViewport.BoundsChanged -= RebuildTiles;
        _gameTimer.ElapsedTimeChanged -= OnElapsedTimeChanged;
        _inputHandler.EventTileFlagClicked -= OnTileFlagClicked;
        ClearTiles();
    }

    public void Tick()
    {
        _gameTimer.Tick(Time.deltaTime);
        _inputHandler.Tick(_gameState == GameState.Playing && _board != null);
    }

    public void RestartGame()
    {
        if (!TryInitBoard())
        {
            return;
        }

        _gameTimer.Reset();
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
        _isFirstReveal = true;
        return true;
    }

    private void OnElapsedTimeChanged(float elapsedSeconds)
    {
        EventElapsedTimeChanged?.Invoke(elapsedSeconds);
    }

    private void OnTileClicked(TileView tileView)
    {
        if (_gameState != GameState.Playing)
        {
            return;
        }

        if (_isFirstReveal)
        {
            _board.EnsureFirstClickIsSafe(tileView.Col, tileView.Row);
            _isFirstReveal = false;
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
        _gameTimer.Stop();
        EventGameFinished?.Invoke(isWin, _gameTimer.ElapsedSeconds);
    }

    private void NotifyFirstUserAction()
    {
        if (_gameTimer.IsRunning)
        {
            return;
        }

        _gameTimer.Start();
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
        return BoardLayout.GetTileIndex(_levelConfig.Cols, col, row);
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

        if (!BoardLayout.TryCalculate(cols, rows, bounds, _tileSpacing, out var layout))
        {
            return;
        }

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                var position = BoardLayout.GetCellCenter(layout, col, row);
                var tileView = Object.Instantiate(_tileViewPrefab, position, Quaternion.identity, _tilesRoot);
                if (!tileView.Init(col, row, layout.CellSize, _tileSprite))
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
