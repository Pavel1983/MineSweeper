using System.Collections.Generic;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    [SerializeField] private ScreensRegistry _screensRegistry;
    [SerializeField] private Transform _screensRoot;
    [SerializeField, ScreenId] private int _initialScreenId = ScreenIds.Start;

    private BaseScreen _currentScreen;
    private readonly Stack<BaseScreen> _stack = new();

    private void Start()
    {
        NavigateTo(_initialScreenId);
    }

    public void NavigateTo(int screenId, ScreenUserData userData = null)
    {
        while (_currentScreen != null && _currentScreen.ScreenId != screenId)
        {
            DestroyScreen(_currentScreen);
            _currentScreen = _stack.Count > 0 ? _stack.Pop() : null;
        }

        if (_currentScreen != null && _currentScreen.ScreenId == screenId)
        {
            ShowScreen(_currentScreen);
            if (userData != null)
            {
                _currentScreen.ApplyUserData(userData);
            }

            return;
        }

        while (_stack.Count > 0)
        {
            DestroyScreen(_stack.Pop());
        }

        DestroyScreen(_currentScreen);
        _currentScreen = CreateScreen(screenId, userData);
    }

    public void Push(int screenId, ScreenUserData userData = null)
    {
        if (_currentScreen != null)
        {
            HideScreen(_currentScreen);
            _stack.Push(_currentScreen);
        }

        _currentScreen = CreateScreen(screenId, userData);
    }

    public bool Pop()
    {
        if (_stack.Count == 0)
        {
            return false;
        }

        DestroyScreen(_currentScreen);
        _currentScreen = _stack.Pop();
        ShowScreen(_currentScreen);
        return true;
    }

    private BaseScreen CreateScreen(int screenId, ScreenUserData userData = null)
    {
        if (!_screensRegistry.TryGetPrefab(screenId, out var prefab))
        {
            Debug.LogError($"Navigation: screen prefab is not found for id {screenId}.");
            return null;
        }

        var screen = Instantiate(prefab, _screensRoot);
        screen.Open(this, userData);
        return screen;
    }

    private static void HideScreen(BaseScreen screen)
    {
        screen.gameObject.SetActive(false);
    }

    private static void ShowScreen(BaseScreen screen)
    {
        screen.gameObject.SetActive(true);
    }

    private static void DestroyScreen(BaseScreen screen)
    {
        if (screen == null)
        {
            return;
        }

        screen.Close();
        Destroy(screen.gameObject);
    }
}
