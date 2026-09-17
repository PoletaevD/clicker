using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Views
{
    [ExecuteAlways]
    public sealed class ResponsiveFrame : MonoBehaviour
    {
        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private RectTransform _safeArea;
        [SerializeField] private RectTransform _frame;

        private void Update()
        {
            if (Screen.width <= 0 || Screen.height <= 0 || _scaler == null || _safeArea == null || _frame == null)
            {
                return;
            }

            var referenceAspect = _scaler.referenceResolution.x / _scaler.referenceResolution.y;
            _scaler.matchWidthOrHeight = Screen.width / (float)Screen.height < referenceAspect ? 0f : 1f;

            var safe = Screen.safeArea;
            _safeArea.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            _safeArea.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            _safeArea.offsetMin = _safeArea.offsetMax = Vector2.zero;

            var width = Mathf.Min(960, Mathf.Max(0, _safeArea.rect.width - 48));

            _frame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }
}
