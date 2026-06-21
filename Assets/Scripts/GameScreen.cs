using FastMerger.Game.View;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : BaseScreen
{
    [SerializeField] private LevelConfig _levelConfig;
    [SerializeField] private BoardViewport _boardViewport;
    [SerializeField] private TileView _tileViewPrefab;
    [SerializeField] private Sprite _tileSprite;
    [SerializeField, Min(0f)] private float _tileSpacing;
    [SerializeField] private FlagView _flagView;
    [SerializeField] private ClockView _clockView;
    [SerializeField] private Button _pauseButton;

    private BoardPresenter _boardPresenter;
    private GameObject _tilesRoot;
    private bool _hasFocus = true;

    private void Awake()
    {
        _pauseButton.onClick.AddListener(OnPauseClicked);
    }

    private void OnDestroy()
    {
        _pauseButton.onClick.RemoveListener(OnPauseClicked);
    }

    private void Update()
    {
        if (!_hasFocus)
        {
            return;
        }

        _boardPresenter?.Tick();
    }

    protected override void OnOpen(ScreenUserData userData)
    {
        _tilesRoot = new GameObject("TilesRoot");

        _boardPresenter = new BoardPresenter(
            _levelConfig,
            _boardViewport,
            _tileViewPrefab,
            _tileSprite,
            _tilesRoot.transform,
            _tileSpacing
        );

        _boardPresenter.EventHudResetRequested += OnHudResetRequested;
        _boardPresenter.EventFlaggedCountChanged += OnFlaggedCountChanged;
        _boardPresenter.EventElapsedTimeChanged += OnElapsedTimeChanged;
        _boardPresenter.EventGameFinished += OnGameFinished;

        if (userData is GameScreenUserData { ShouldRestart: true })
        {
            _boardPresenter.RestartGame();
            return;
        }

        SyncHud();
    }

    protected override void OnClose()
    {
        if (_boardPresenter != null)
        {
            _boardPresenter.EventHudResetRequested -= OnHudResetRequested;
            _boardPresenter.EventFlaggedCountChanged -= OnFlaggedCountChanged;
            _boardPresenter.EventElapsedTimeChanged -= OnElapsedTimeChanged;
            _boardPresenter.EventGameFinished -= OnGameFinished;
            _boardPresenter.Dispose();
            _boardPresenter = null;
        }

        Destroy(_tilesRoot);
        _tilesRoot = null;
    }

    protected override void OnApplyUserData(ScreenUserData userData)
    {
        if (userData is GameScreenUserData { ShouldRestart: true })
        {
            _boardPresenter.RestartGame();
        }
    }

    protected override void OnFocusLost()
    {
        _hasFocus = false;
    }

    protected override void OnFocusGained()
    {
        _hasFocus = true;
    }

    private void SyncHud()
    {
        OnHudResetRequested(_boardPresenter.MinesCount);
        OnFlaggedCountChanged(_boardPresenter.FlaggedCount);
        OnElapsedTimeChanged(_boardPresenter.ElapsedSeconds);
    }

    private void OnHudResetRequested(int minesCount)
    {
        _flagView.Init(minesCount);
        _clockView.ResetView();
    }

    private void OnFlaggedCountChanged(int flaggedCount)
    {
        _flagView.SetFlaggedCount(flaggedCount);
    }

    private void OnElapsedTimeChanged(float elapsedSeconds)
    {
        _clockView.SetTime(elapsedSeconds);
    }

    private void OnGameFinished(bool isWin, float elapsedSeconds)
    {
        Navigation.Push(ScreenIds.Results, new ResultsScreenUserData
        {
            IsWin = isWin,
            ElapsedSeconds = elapsedSeconds
        });
    }

    private void OnPauseClicked()
    {
        Navigation.Push(ScreenIds.Settings);
    }
}
