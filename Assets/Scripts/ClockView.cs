using TMPro;
using UnityEngine;

public class ClockView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;

    private int _lastDisplayedSeconds = -1;

    private void Awake()
    {
        if (_label == null)
        {
            _label = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void ResetView()
    {
        _lastDisplayedSeconds = -1;
        SetTime(0f);
    }

    public void SetTime(float elapsedSeconds)
    {
        if (_label == null)
        {
            return;
        }

        var totalSeconds = Mathf.FloorToInt(elapsedSeconds);
        if (totalSeconds == _lastDisplayedSeconds)
        {
            return;
        }

        _lastDisplayedSeconds = totalSeconds;
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        _label.text = $"{minutes:00}:{seconds:00}";
    }
}
