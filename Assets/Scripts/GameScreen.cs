using FastMerger.Game.View;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : BaseScreen, IBoardHud
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

        _boardPresenter.BindHud(this);

        if (userData is GameScreenUserData { ShouldRestart: true })
        {
            _boardPresenter.RestartGame();
            return;
        }

        _boardPresenter.SyncHud();
    }

    protected override void OnClose()
    {
        _boardPresenter.ClearHud();
        _boardPresenter.Dispose();
        _boardPresenter = null;

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

    public void ResetHud(int minesCount)
    {
        _flagView.Init(minesCount);
        _clockView.ResetView();
    }

    public void UpdateFlaggedCount(int flaggedCount)
    {
        _flagView.SetFlaggedCount(flaggedCount);
    }

    public void UpdateElapsedTime(float elapsedSeconds)
    {
        _clockView.SetTime(elapsedSeconds);
    }

    private void OnPauseClicked()
    {
        Navigation.Push(ScreenIds.Settings);
    }
}
