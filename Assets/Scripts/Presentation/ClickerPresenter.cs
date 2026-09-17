using System;
using Clicker.Configuration;
using Clicker.Domain;
using UniRx;
using UnityEngine;
using Zenject;

namespace Clicker.Presentation
{
    public sealed class ClickerPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly ClickerModel _model;
        private readonly IClickerView _view;
        private readonly IShellView _shell;
        private readonly GameSettings _settings;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public ClickerPresenter(ClickerModel model, IClickerView view, IShellView shell, GameSettings settings)
        {
            _model = model;
            _view = view;
            _shell = shell;
            _settings = settings;
        }

        public void Initialize()
        {
            _view.SetEconomy(_settings.CurrencyPerClick, _settings.EnergyPerClick, _settings.EnergyPerRecharge, _settings.RechargeInterval, _settings.AutoClickInterval);

            _view.Clicked.Subscribe(_ =>
            {
                if (!_model.TryCollect(ClickSource.Manual))
                {
                    _view.ShowInsufficientEnergy();
                }
            }).AddTo(_subscriptions);

            _model.Currency.CombineLatest(_model.Energy, (currency, energy) => (currency, energy)).Subscribe(balance =>
            {
                _shell.SetBalance(balance.currency, balance.energy, _settings.MaximumEnergy);
                _view.SetEnergy(balance.energy, _settings.MaximumEnergy, balance.energy >= _settings.EnergyPerClick);
            }).AddTo(_subscriptions);

            _model.AutoProgress.CombineLatest(_model.RechargeProgress, (auto, recharge) => (auto, recharge))
                .Subscribe(timers => _view.SetTimers(timers.auto, timers.recharge)).AddTo(_subscriptions);

            _model.Rewards.Subscribe(_view.PlayReward).AddTo(_subscriptions);
        }

        public void Tick()
        {
            _model.Advance(Time.unscaledDeltaTime);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
