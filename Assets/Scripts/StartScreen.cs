using UnityEngine;
using UnityEngine.UI;

public class StartScreen : BaseScreen
{
    [SerializeField] private Button _startButton;

    private void Awake()
    {
        _startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnDestroy()
    {
        _startButton.onClick.RemoveListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        Navigation.NavigateTo(ScreenIds.Game, new GameScreenUserData { ShouldRestart = true });
    }
}
