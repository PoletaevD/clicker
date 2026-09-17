using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Clicker.Views
{
    public sealed class UIParticle : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _particleColor;

        private Sequence _animation;

        public void Play(Vector2 origin, Vector2 velocity, float duration, Action completed)
        {
            var rect = (RectTransform)transform;
            rect.anchoredPosition = origin;
            rect.localScale = Vector3.one;

            _image.color = _particleColor;
            var time = 0f;

            _animation = DOTween.Sequence().SetUpdate(true);

            _animation.Append(DOTween.To(() => time, value =>
            {
                time = value;
                rect.anchoredPosition = origin + velocity * time + Vector2.down * (200 * time * time);
                var color = _image.color;
                color.a = 1 - time / duration;
                _image.color = color;
            }, duration, duration).SetEase(Ease.Linear));

            _animation.OnComplete(() => completed());
        }

        public void Stop()
        {
            _animation?.Kill();

            _animation = null;
        }

        private void OnDisable()
        {
            Stop();
        }

        public sealed class Pool : MonoMemoryPool<UIParticle>
        {
            protected override void OnDespawned(UIParticle item)
            {
                item.Stop();

                base.OnDespawned(item);
            }
        }
    }
}
