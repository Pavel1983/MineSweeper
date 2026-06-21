using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScreen : BaseScreen
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private TextMeshProUGUI _resultLabel;
    [SerializeField] private TextMeshProUGUI _timeLabel;

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

    protected override void OnOpen(ScreenUserData userData)
    {
        ApplyResult(userData);
    }

    protected override void OnApplyUserData(ScreenUserData userData)
    {
        ApplyResult(userData);
    }

    private void ApplyResult(ScreenUserData userData)
    {
        if (userData is not ResultsScreenUserData resultData)
        {
            return;
        }

        if (_resultLabel != null)
        {
            _resultLabel.text = resultData.IsWin ? "You win!" : "You lose!";
        }

        if (_timeLabel != null)
        {
            var totalSeconds = Mathf.FloorToInt(resultData.ElapsedSeconds);
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;
            _timeLabel.text = $"Time: {minutes:00}:{seconds:00}";
        }
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
