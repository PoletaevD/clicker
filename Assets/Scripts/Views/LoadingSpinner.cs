using DG.Tweening;
using UnityEngine;

namespace Clicker.Views
{
    public sealed class LoadingSpinner : MonoBehaviour
    {
        private Tween _tween;

        private void OnEnable()
        {
            _tween = transform.DOLocalRotate(new Vector3(0, 0, -360), .85f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetUpdate(true);
        }

        private void OnDisable()
        {
            _tween?.Kill();
            transform.localRotation = Quaternion.identity;
        }
    }
}
