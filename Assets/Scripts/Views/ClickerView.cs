using System;
using System.Collections.Generic;
using Clicker.Configuration;
using Clicker.Domain;
using Clicker.Presentation;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Clicker.Views
{
    public sealed class ClickerView : MonoBehaviour, IClickerView
    {
        [SerializeField] private Button _collectButton;
        [SerializeField] private RectTransform _coin;
        [SerializeField] private RectTransform _effectsRoot;
        [SerializeField] private Image _energyFill;
        [SerializeField] private Image _autoFill;
        [SerializeField] private TMP_Text _energyText;
        [SerializeField] private TMP_Text _rewardHint;
        [SerializeField] private TMP_Text _autoHint;
        [SerializeField] private TMP_Text _rechargeHint;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private AudioSource _audio;

        private readonly HashSet<FloatingReward> _activeRewards = new HashSet<FloatingReward>();
        private readonly HashSet<UIParticle> _activeParticles = new HashSet<UIParticle>();
        private FloatingReward.Pool _rewards;
        private UIParticle.Pool _particles;
        private GameSettings _settings;
        private Tween _punch;
        private float _autoSeconds;
        private float _rechargeSeconds;
        private int _rechargeAmount;

        public RectTransform EffectsRoot => _effectsRoot;
        public IObservable<Unit> Clicked => _collectButton.OnClickAsObservable();

        [Inject]
        private void Construct(FloatingReward.Pool rewards, UIParticle.Pool particles, GameSettings settings)
        {
            _rewards = rewards;
            _particles = particles;
            _settings = settings;
        }

        public void SetEconomy(int reward, int cost, int recharge, float rechargeSeconds, float autoSeconds)
        {
            _rewardHint.text = $"+{reward} монета за нажатие  /  −{cost} энергии";

            _autoSeconds = autoSeconds;
            _rechargeSeconds = rechargeSeconds;
            _rechargeAmount = recharge;
        }

        public void SetEnergy(int energy, int maximum, bool canCollect)
        {
            _energyText.text = $"{energy} / {maximum}";
            _energyFill.fillAmount = energy / (float)maximum;

            _status.text = canCollect ? "НАЖМИ, ЧТОБЫ СОБРАТЬ" : "ЭНЕРГИЯ ВОССТАНАВЛИВАЕТСЯ";
        }

        public void SetTimers(float autoProgress, float rechargeProgress)
        {
            _autoFill.fillAmount = autoProgress;
            _autoHint.text = $"Автосбор через {Mathf.CeilToInt((1 - autoProgress) * _autoSeconds)} с";

            var rechargeRemaining = Mathf.CeilToInt((1 - rechargeProgress) * _rechargeSeconds);

            _rechargeHint.text = $"+{_rechargeAmount} через {rechargeRemaining} с";
        }

        public void PlayReward(ClickReward reward)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            _punch?.Kill();

            _coin.localScale = Vector3.one;
            _punch = _coin.DOPunchScale(Vector3.one * -.12f, .28f, 5, .5f).SetUpdate(true);

            if (_settings.ClickSound != null)
            {
                _audio.PlayOneShot(_settings.ClickSound, _settings.ClickVolume);
            }

            var origin = (Vector2)_effectsRoot.InverseTransformPoint(_coin.position);
            var floating = _rewards.Spawn();

            _activeRewards.Add(floating);

            floating.Play(reward.Amount, origin + Vector2.up * 50, _settings.RewardFlightDuration, () =>
            {
                _activeRewards.Remove(floating);
                _rewards.Despawn(floating);
            });

            for (var i = 0; i < _settings.BurstParticleCount; i++)
            {
                var particle = _particles.Spawn();
                _activeParticles.Add(particle);

                var angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                particle.Play(origin + direction * 85, direction * UnityEngine.Random.Range(120, 280), .6f, () =>
                {
                    _activeParticles.Remove(particle);
                    _particles.Despawn(particle);
                });
            }
        }

        public void ShowInsufficientEnergy()
        {
            _status.text = "НЕДОСТАТОЧНО ЭНЕРГИИ";

            _punch?.Kill();

            _coin.localScale = Vector3.one;
            _punch = _coin.DOPunchScale(Vector3.one * .035f, .18f).SetUpdate(true);
        }

        private void OnDisable()
        {
            _punch?.Kill();

            if (_coin != null)
            {
                _coin.localScale = Vector3.one;
            }

            foreach (var item in _activeRewards)
            {
                _rewards.Despawn(item);
            }

            foreach (var item in _activeParticles)
            {
                _particles.Despawn(item);
            }

            _activeRewards.Clear();
            _activeParticles.Clear();
        }
    }
}
