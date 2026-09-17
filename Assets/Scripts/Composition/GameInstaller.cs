using Clicker.Configuration;
using Clicker.Domain;
using Clicker.Infrastructure;
using Clicker.Presentation;
using Clicker.Views;
using UnityEngine;
using Zenject;

namespace Clicker.Composition
{
    public sealed class GameInstaller : MonoInstaller
    {
        [SerializeField] private GameSettings _settings;
        [SerializeField] private ShellView _shell;
        [SerializeField] private ClickerView _clicker;
        [SerializeField] private WeatherView _weather;
        [SerializeField] private BreedsView _breeds;
        [SerializeField] private BreedPopupView _popup;
        [SerializeField] private BreedRowView _breedRowPrefab;
        [SerializeField] private FloatingReward _rewardPrefab;
        [SerializeField] private UIParticle _particlePrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(_settings);

            Container.Bind<IShellView>().FromInstance(_shell);
            Container.Bind<IClickerView>().FromInstance(_clicker);
            Container.Bind<IWeatherView>().FromInstance(_weather);
            Container.Bind<IBreedsView>().FromInstance(_breeds);
            Container.Bind<IBreedPopupView>().FromInstance(_popup);
            Container.BindInterfacesAndSelfTo<ClickerModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<SerialRequestQueue>().AsSingle();
            Container.Bind<IHttpTransport>().To<UnityHttpTransport>().AsSingle();
            Container.Bind<ApiParser>().AsSingle();
            Container.BindInterfacesTo<ApiClient>().AsSingle();

            Container.BindFactory<BreedRowView, BreedRowView.Factory>()
                .FromComponentInNewPrefab(_breedRowPrefab).UnderTransform(_breeds.Content);
            Container.BindFactory<Breed, int, BreedRowPresenter, BreedRowPresenter.Factory>();
            Container.BindMemoryPool<FloatingReward, FloatingReward.Pool>().WithInitialSize(12)
                .FromComponentInNewPrefab(_rewardPrefab).UnderTransform(_clicker.EffectsRoot);
            Container.BindMemoryPool<UIParticle, UIParticle.Pool>().WithInitialSize(80)
                .FromComponentInNewPrefab(_particlePrefab).UnderTransform(_clicker.EffectsRoot);

            Container.BindInterfacesAndSelfTo<ClickerPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<WeatherPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<BreedsPresenter>().AsSingle();
            Container.BindInterfacesTo<NavigationPresenter>().AsSingle();

            Container.BindExecutionOrder<ClickerPresenter>(10);
            Container.BindExecutionOrder<WeatherPresenter>(20);
            Container.BindExecutionOrder<BreedsPresenter>(20);
            Container.BindExecutionOrder<NavigationPresenter>(30);
        }
    }
}
