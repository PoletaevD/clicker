using System;
using Clicker.Domain;
using Clicker.Views;
using UniRx;
using Zenject;

namespace Clicker.Presentation
{
    public sealed class BreedRowPresenter : IDisposable
    {
        private readonly BreedRowView _view;

        public string Id { get; }
        public IObservable<Unit> Clicked => _view.Clicked;

        public BreedRowPresenter(Breed breed, int index, BreedRowView.Factory factory)
        {
            Id = breed.Id;
            _view = factory.Create();
            _view.Render(index, breed.Name);
        }

        public void SetLoading(bool loading)
        {
            _view.SetLoading(loading);
        }

        public void Dispose()
        {
            if (_view != null)
            {
                UnityEngine.Object.Destroy(_view.gameObject);
            }
        }

        public sealed class Factory : PlaceholderFactory<Breed, int, BreedRowPresenter>
        {
        }
    }
}
