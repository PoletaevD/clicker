using System;
using Clicker.Configuration;
using UniRx;

namespace Clicker.Domain
{
    public enum ClickSource
    {
        Manual,
        Automatic
    }

    public readonly struct ClickReward
    {
        public int Amount { get; }
        public ClickSource Source { get; }

        public ClickReward(int amount, ClickSource source)
        {
            Amount = amount;
            Source = source;
        }
    }

    /// <summary>Session economy. Both click sources use the same atomic spend/reward operation.</summary>
    public sealed class ClickerModel : IDisposable
    {
        private readonly GameSettings _settings;
        private readonly ReactiveProperty<long> _currency;
        private readonly ReactiveProperty<int> _energy;
        private readonly ReactiveProperty<float> _autoProgress = new ReactiveProperty<float>();
        private readonly ReactiveProperty<float> _rechargeProgress = new ReactiveProperty<float>();
        private readonly Subject<ClickReward> _rewards = new Subject<ClickReward>();
        private double _autoElapsed;
        private double _rechargeElapsed;

        public IReadOnlyReactiveProperty<long> Currency => _currency;
        public IReadOnlyReactiveProperty<int> Energy => _energy;
        public IReadOnlyReactiveProperty<float> AutoProgress => _autoProgress;
        public IReadOnlyReactiveProperty<float> RechargeProgress => _rechargeProgress;
        public IObservable<ClickReward> Rewards => _rewards;

        public ClickerModel(GameSettings settings)
        {
            _settings = settings;
            _currency = new ReactiveProperty<long>(Math.Max(0, settings.InitialCurrency));
            _energy = new ReactiveProperty<int>(Math.Max(0, Math.Min(settings.InitialEnergy, settings.MaximumEnergy)));
        }

        public bool TryCollect(ClickSource source)
        {
            if (_energy.Value < _settings.EnergyPerClick
                || _currency.Value > long.MaxValue - _settings.CurrencyPerClick)
            {
                return false;
            }

            _energy.Value -= _settings.EnergyPerClick;
            _currency.Value += _settings.CurrencyPerClick;
            _rewards.OnNext(new ClickReward(_settings.CurrencyPerClick, source));
            return true;
        }

        /// <summary>Advances in chronological order; recharge precedes auto-collect on equal timestamps.</summary>
        public void Advance(double seconds)
        {
            if (seconds <= 0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
            {
                return;
            }

            var autoInterval = Math.Max(0.1, _settings.AutoClickInterval);
            var rechargeInterval = Math.Max(0.1, _settings.RechargeInterval);
            while (seconds > 0.0000001)
            {
                var step = Math.Min(seconds,
                    Math.Min(autoInterval - _autoElapsed, rechargeInterval - _rechargeElapsed));
                _autoElapsed += step;
                _rechargeElapsed += step;
                seconds -= step;
                if (_rechargeElapsed >= rechargeInterval - 0.0000001)
                {
                    _rechargeElapsed = 0;
                    _energy.Value = (int)Math.Min(_settings.MaximumEnergy,
                        (long)_energy.Value + _settings.EnergyPerRecharge);
                }

                if (_autoElapsed >= autoInterval - 0.0000001)
                {
                    _autoElapsed = 0;
                    TryCollect(ClickSource.Automatic);
                }
            }

            _autoProgress.Value = (float)(_autoElapsed / autoInterval);
            _rechargeProgress.Value = (float)(_rechargeElapsed / rechargeInterval);
        }

        public void Dispose()
        {
            _currency.Dispose();
            _energy.Dispose();
            _autoProgress.Dispose();
            _rechargeProgress.Dispose();
            _rewards.Dispose();
        }
    }
}
