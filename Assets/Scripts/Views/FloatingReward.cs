using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Clicker.Views
{
    public sealed class FloatingReward : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private CanvasGroup _group;

        private Sequence _animation;

        public void Play(int amount, Vector2 origin, float duration, Action completed)
        {
            var rect = (RectTransform)transform;
            rect.anchoredPosition = origin;
            rect.localScale = Vector3.one * .75f;

            _label.text = "+" + amount;

            _group.alpha = 1;

            _animation = DOTween.Sequence().SetUpdate(true)
                .Append(DOTween.To(() => rect.anchoredPosition, position => rect.anchoredPosition = position,
                    origin + Vector2.up * 170, duration).SetEase(Ease.OutCubic))
                .Join(rect.DOScale(1.2f, duration * .4f).SetEase(Ease.OutBack))
                .Insert(duration * .4f, DOTween.To(() => _group.alpha, a => _group.alpha = a, 0, duration * .6f))
                .OnComplete(() => completed());
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

        public sealed class Pool : MonoMemoryPool<FloatingReward>
        {
            protected override void OnDespawned(FloatingReward item)
            {
                item.Stop();

                base.OnDespawned(item);
            }
        }
    }
}
