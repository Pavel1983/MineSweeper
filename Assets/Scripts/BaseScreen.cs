using UnityEngine;

public abstract class BaseScreen : MonoBehaviour
{
    [ScreenId]
    public int ScreenId;

    protected Navigation Navigation { get; private set; }

    internal void Open(Navigation navigation, ScreenUserData userData)
    {
        Navigation = navigation;
        OnOpen(userData);
    }

    internal void Close()
    {
        OnClose();
        Navigation = null;
    }

    internal void ApplyUserData(ScreenUserData userData)
    {
        OnApplyUserData(userData);
    }

    protected virtual void OnOpen(ScreenUserData userData)
    {
    }

    protected virtual void OnClose()
    {
    }

    protected virtual void OnApplyUserData(ScreenUserData userData)
    {
    }
}
