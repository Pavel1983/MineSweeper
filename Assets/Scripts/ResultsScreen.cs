using UnityEngine;
using UnityEngine.UI;

public class ResultsScreen : BaseScreen
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;

    private void Awake()
    {
        _restartButton.onClick.AddListener(OnRestartClicked);
        _exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        _restartButton.onClick.RemoveListener(OnRestartClicked);
        _exitButton.onClick.RemoveListener(OnExitClicked);
    }

    private void OnRestartClicked()
    {
        Navigation.NavigateTo(ScreenIds.Game, new GameScreenUserData { ShouldRestart = true });
    }

    private void OnExitClicked()
    {
        Navigation.NavigateTo(ScreenIds.Start);
    }
}
