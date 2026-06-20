using UnityEngine;

namespace FastMerger.Game.Runtime
{
    /// <summary>
    /// Подгоняет RectTransform под Screen.safeArea при смене разрешения и ориентации.
    /// Вешается на контейнер UI
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool _applyTop = true;
        [SerializeField] private bool _applyBottom = true;
        [SerializeField] private bool _applyLeft = true;
        [SerializeField] private bool _applyRight = true;

        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            ApplySafeArea();
        }

        private void OnEnable()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea == _lastSafeArea
                && Screen.width == _lastScreenSize.x
                && Screen.height == _lastScreenSize.y
                && Screen.orientation == _lastOrientation)
            {
                return;
            }

            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (_rectTransform == null)
            {
                _rectTransform = (RectTransform)transform;
            }

            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                return;
            }

            var safeArea = Screen.safeArea;
            var screenSize = new Vector2(screenWidth, screenHeight);

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            if (!_applyLeft)
            {
                anchorMin.x = 0f;
            }

            if (!_applyBottom)
            {
                anchorMin.y = 0f;
            }

            if (!_applyRight)
            {
                anchorMax.x = screenSize.x;
            }

            if (!_applyTop)
            {
                anchorMax.y = screenSize.y;
            }

            _rectTransform.anchorMin = anchorMin / screenSize;
            _rectTransform.anchorMax = anchorMax / screenSize;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(screenWidth, screenHeight);
            _lastOrientation = Screen.orientation;
        }
    }
}
