using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileView : MonoBehaviour, IPointerClickHandler
{
    public event Action<TileView> EventClick;
    
    [SerializeField] private SpriteRenderer _renderer;
    
    public void Init(float worldSize, Sprite sprite)
    {
        _renderer.sprite = sprite;

        var spriteSize = sprite.bounds.size.x;
        var scale = spriteSize > 0f ? worldSize / spriteSize : worldSize;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EventClick?.Invoke(this);
    }
}
