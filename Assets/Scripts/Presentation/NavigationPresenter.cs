using System;
using UniRx;
using Zenject;

namespace Clicker.Presentation
{
    public sealed class NavigationPresenter : IInitializable, IDisposable
    {
        private readonly IShellView _view;
        private readonly WeatherPresenter _weather;
        private readonly BreedsPresenter _breeds;
        private IDisposable _subscription;
        private AppTab? _current;

        public NavigationPresenter(IShellView view, WeatherPresenter weather, BreedsPresenter breeds)
        {
            _view = view;
            _weather = weather;
            _breeds = breeds;
        }

        public void Initialize()
        {
            _subscription = _view.TabSelected.Subscribe(Select);

            Select(AppTab.Clicker);
        }

        public void Select(AppTab tab)
        {
            if (_current == tab)
            {
                return;
            }

            _weather.Deactivate();
            _breeds.Deactivate();
            _current = tab;
            _view.SelectTab(tab);

            if (tab == AppTab.Weather)
            {
                _weather.Activate();
            }

            if (tab == AppTab.Breeds)
            {
                _breeds.Activate();
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
