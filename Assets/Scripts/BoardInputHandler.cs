using System;
using FastMerger.Game.View;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardInputHandler
{
    public event Action<TileView> EventTileFlagClicked;

    private readonly BoardViewport _boardViewport;

    public BoardInputHandler(BoardViewport boardViewport)
    {
        _boardViewport = boardViewport;
    }

    public void Tick(bool isEnabled)
    {
        if (!isEnabled
            || Mouse.current == null
            || !Mouse.current.rightButton.wasPressedThisFrame)
        {
            return;
        }

        var camera = _boardViewport.TargetCamera;
        if (camera == null)
        {
            Debug.LogError("BoardInputHandler: camera is not set up.");
            return;
        }

        var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider == null || !hit.collider.TryGetComponent(out TileView tileView))
        {
            return;
        }

        EventTileFlagClicked?.Invoke(tileView);
    }
}
