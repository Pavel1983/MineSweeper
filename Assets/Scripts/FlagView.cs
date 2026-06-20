using TMPro;
using UnityEngine;

public class FlagView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;

    private int _minesCount;

    private void Awake()
    {
        if (_label == null)
        {
            _label = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void Init(int minesCount)
    {
        _minesCount = minesCount;
        SetFlaggedCount(0);
    }

    public void SetFlaggedCount(int flaggedCount)
    {
        if (_label == null)
        {
            Debug.LogError("FlagView: label is not set up.");
            return;
        }

        _label.text = (_minesCount - flaggedCount).ToString();
    }
}
