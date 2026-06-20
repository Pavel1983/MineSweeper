using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : BaseScreen
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _exitButton;

    private void Awake()
    {
        _restartButton.onClick.AddListener(OnRestartClicked);
        _continueButton.onClick.AddListener(OnContinueClicked);
        _exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        _restartButton.onClick.RemoveListener(OnRestartClicked);
        _continueButton.onClick.RemoveListener(OnContinueClicked);
        _exitButton.onClick.RemoveListener(OnExitClicked);
    }

    private void OnRestartClicked()
    {
        Navigation.NavigateTo(ScreenIds.Game, new GameScreenUserData { ShouldRestart = true });
    }

    private void OnContinueClicked()
    {
        Navigation.Pop();
    }

    private void OnExitClicked()
    {
        Navigation.NavigateTo(ScreenIds.Start);
    }
}
