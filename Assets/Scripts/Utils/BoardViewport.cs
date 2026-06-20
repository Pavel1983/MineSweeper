using System;
using UnityEngine;

namespace FastMerger.Game.View
{
    /// <summary>
    /// Область на сцене, внутри которой рисуется сетка тайлов.
    /// Если задан RectTransform (обычно элемент Canvas), границы берутся из него.
    /// Иначе — из долей видимой области ортографической камеры.
    /// </summary>
    [ExecuteAlways]
    public class BoardViewport : MonoBehaviour
    {
        public enum AnchorMode
        {
            Center,
            BottomLeft,
        }

        [SerializeField] private Camera _camera;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private AnchorMode _anchor = AnchorMode.Center;
        [SerializeField] private bool _clipContents = true;
        [SerializeField] private SpriteMask _spriteMask;

        private static Sprite _defaultMaskSprite;

        private int _cachedColumns = 1;
        private int _cachedVisibleRows = 1;
        private Bounds _lastBounds;
        private bool _hasLastBounds;

        public event Action BoundsChanged;

        public Camera TargetCamera => _camera != null ? _camera : Camera.main;
        public RectTransform BoundsRect => _rectTransform;
        public bool UsesRectTransform => _rectTransform != null;
        public bool ClipContents => _clipContents;

        private void OnEnable()
        {
            RefreshClipMask();
        }

        private void OnValidate()
        {
            RefreshClipMask();
            NotifyBoundsChangedIfNeeded();
        }

        // private void LateUpdate()
        // {
        //     RefreshClipMask();
        //     NotifyBoundsChangedIfNeeded();
        // }

        /// <summary>
        /// Возвращает bounds игровой области. Без RectTransform — из долей видимой области камеры.
        /// columns и visibleRows нужны только для кэша маски/гизмо при смене конфига.
        /// </summary>
        public bool TryGetWorldBounds(int columns, int visibleRows, out Bounds bounds)
        {
            _cachedColumns = Mathf.Max(1, columns);
            _cachedVisibleRows = Mathf.Max(1, visibleRows);
            
            return TryGetBoundsFromRectTransform(out bounds);
        }
        
        private void RefreshClipMask()
        {
            if (!_clipContents)
            {
                if (_spriteMask != null)
                {
                    _spriteMask.gameObject.SetActive(false);
                }

                return;
            }

            if (!TryGetWorldBounds(_cachedColumns, _cachedVisibleRows, out var bounds))
            {
                return;
            }

            EnsureSpriteMask();
            _spriteMask.gameObject.SetActive(true);
            _spriteMask.sprite = GetDefaultMaskSprite();
            _spriteMask.transform.position = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);
            _spriteMask.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);
        }

        private void EnsureSpriteMask()
        {
            if (_spriteMask != null)
            {
                return;
            }

            var maskObject = new GameObject("ClipMask");
            maskObject.transform.SetParent(transform, worldPositionStays: true);
            _spriteMask = maskObject.AddComponent<SpriteMask>();
            _spriteMask.isCustomRangeActive = true;
            _spriteMask.backSortingOrder = -1;
            _spriteMask.frontSortingOrder = 1;
        }

        private static Sprite GetDefaultMaskSprite()
        {
            if (_defaultMaskSprite != null)
            {
                return _defaultMaskSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;

            _defaultMaskSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 1f
            );
            _defaultMaskSprite.hideFlags = HideFlags.HideAndDontSave;

            return _defaultMaskSprite;
        }

        private bool TryGetBoundsFromRectTransform(out Bounds bounds)
        {
            var cam = TargetCamera;
            if (cam == null || _rectTransform == null)
            {
                bounds = default;
                return false;
            }

            var corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);

            var canvas = _rectTransform.GetComponentInParent<Canvas>();
            var uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            var planeDistance = Mathf.Abs(cam.transform.position.z);
            var min = new Vector3(float.MaxValue, float.MaxValue, 0f);
            var max = new Vector3(float.MinValue, float.MinValue, 0f);

            for (var i = 0; i < corners.Length; i++)
            {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[i]);
                var worldPoint = cam.ScreenToWorldPoint(
                    new Vector3(screenPoint.x, screenPoint.y, planeDistance)
                );
                worldPoint.z = 0f;

                min = Vector3.Min(min, worldPoint);
                max = Vector3.Max(max, worldPoint);
            }

            var size = max - min;
            if (size.x <= 0f || size.y <= 0f)
            {
                bounds = default;
                return false;
            }

            bounds = new Bounds((min + max) * 0.5f, size);
            return true;
        }

        private void NotifyBoundsChangedIfNeeded()
        {
            if (!TryGetWorldBounds(_cachedColumns, _cachedVisibleRows, out var bounds))
            {
                if (!_hasLastBounds)
                {
                    return;
                }

                _hasLastBounds = false;
                BoundsChanged?.Invoke();
                return;
            }

            if (_hasLastBounds && BoundsApproximatelyEqual(_lastBounds, bounds))
            {
                return;
            }

            _lastBounds = bounds;
            _hasLastBounds = true;
            BoundsChanged?.Invoke();
        }

        private static bool BoundsApproximatelyEqual(Bounds a, Bounds b)
        {
            return Vector3.Distance(a.center, b.center) < 0.0001f
                && Vector3.Distance(a.size, b.size) < 0.0001f;
        }

        private Vector3 GetAnchorPoint(Vector2 size)
        {
            var position = transform.position;

            return _anchor switch
            {
                AnchorMode.BottomLeft => position + new Vector3(size.x * 0.5f, size.y * 0.5f, 0f),
                _ => position,
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (!TryGetWorldBounds(_cachedColumns, _cachedVisibleRows, out var bounds))
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawCube(bounds.center, bounds.size);

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
