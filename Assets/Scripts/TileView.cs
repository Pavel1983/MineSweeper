using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileView : MonoBehaviour, IPointerClickHandler
{
    private const float LocalTileSize = 1f;
    private const float MineFillRatio = 0.65f;

    public event Action<TileView> EventClick;

    public int Col { get; private set; }
    public int Row { get; private set; }

    [SerializeField] private Color _hiddenColor;
    [SerializeField] private Color _revealedColor;
    [SerializeField] private Color _mineColor = new(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private SpriteRenderer _mineRenderer;
    [SerializeField] private TextMeshPro _label;
    [SerializeField] private BoxCollider2D _collider;

    public bool Init(int col, int row, float worldSize, Sprite sprite)
    {
        if (_renderer == null)
        {
            Debug.LogError("TileView: _renderer is not set up.");
            return false;
        }

        if (_mineRenderer == null)
        {
            Debug.LogError("TileView: _mineRenderer is not set up.");
            return false;
        }

        if (_mineRenderer.sprite == null)
        {
            Debug.LogError("TileView: mine sprite is not set up.");
            return false;
        }

        if (_label == null)
        {
            Debug.LogError("TileView: _label is not set up.");
            return false;
        }

        if (_collider == null)
        {
            Debug.LogError("TileView: _collider is not set up.");
            return false;
        }

        if (sprite == null)
        {
            Debug.LogError("TileView: tile sprite is not set up.");
            return false;
        }

        Col = col;
        Row = row;

        _renderer.sprite = sprite;

        var spriteSize = sprite.bounds.size.x;
        if (spriteSize > 0f)
        {
            _renderer.transform.localScale = Vector3.one * (LocalTileSize / spriteSize);
            FitMineSprite(spriteSize);
        }

        transform.localScale = new Vector3(worldSize, worldSize, 1f);
        _collider.size = Vector2.one * LocalTileSize;

        ShowHidden();
        return true;
    }

    public void ShowHidden()
    {
        _renderer.color = _hiddenColor;
        SetMineVisible(false);
        SetLabel(string.Empty);
    }

    public void ShowRevealed(int neighborMines, bool isMine)
    {
        gameObject.name = "tile_revealed";

        _renderer.color = isMine ? _mineColor : _revealedColor;
        SetMineVisible(isMine);
        SetLabel(isMine || neighborMines <= 0 ? string.Empty : neighborMines.ToString());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EventClick?.Invoke(this);
    }

    private void FitMineSprite(float backgroundSpriteSize)
    {
        var mineSpriteSize = _mineRenderer.sprite.bounds.size.x;
        _mineRenderer.transform.localScale = Vector3.one * (MineFillRatio * backgroundSpriteSize / mineSpriteSize);
    }

    private void SetMineVisible(bool visible)
    {
        _mineRenderer.gameObject.SetActive(visible);
    }

    private void SetLabel(string text)
    {
        _label.text = text;
        _label.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }
}
