using System;
using System.Collections.Generic;
using System.Threading;
using Clicker.Infrastructure;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Clicker.Presentation
{
    public sealed class BreedsPresenter : IDisposable
    {
        private readonly IBreedsView _view;
        private readonly IBreedPopupView _popup;
        private readonly IBreedsApi _api;
        private readonly BreedRowPresenter.Factory _factory;
        private readonly List<BreedRowPresenter> _rows = new List<BreedRowPresenter>();
        private readonly CompositeDisposable _rowSubscriptions = new CompositeDisposable();
        private readonly IDisposable _retrySubscription;
        private CancellationTokenSource _session;
        private CancellationTokenSource _selection;

        public BreedsPresenter(IBreedsView view, IBreedPopupView popup, IBreedsApi api,
            BreedRowPresenter.Factory factory)
        {
            _view = view;
            _popup = popup;
            _api = api;
            _factory = factory;
            _retrySubscription = view.RetryClicked.Subscribe(_ => Activate());
        }

        public void Activate()
        {
            Deactivate();

            _session = new CancellationTokenSource();

            _view.SetVisible(true);

            LoadListAsync(_session.Token).Forget();
        }

        public void Deactivate()
        {
            EndSession();

            _popup.Hide();

            _view.SetLoading(false);
            _view.SetVisible(false);
        }

        private void EndSession()
        {
            CancelSelection(false);

            if (_session != null)
            {
                _session.Cancel();
                _session.Dispose();

                _session = null;
            }

            _rowSubscriptions.Clear();

            foreach (var row in _rows)
            {
                row.Dispose();
            }

            _rows.Clear();
        }

        private async UniTaskVoid LoadListAsync(CancellationToken token)
        {
            _view.SetLoading(true);

            try
            {
                var breeds = await _api.GetBreedsAsync(token);
                token.ThrowIfCancellationRequested();

                for (var i = 0; i < breeds.Count; i++)
                {
                    var row = _factory.Create(breeds[i], i + 1);

                    _rows.Add(row);

                    row.Clicked.Subscribe(_ => Select(row)).AddTo(_rowSubscriptions);
                }

                _view.ShowCount(breeds.Count);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                if (!token.IsCancellationRequested)
                {
                    Debug.LogWarning("Breed list request: " + exception.Message);

                    _view.ShowError("Не удалось загрузить породы. Проверьте соединение и повторите.");
                }
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    _view.SetLoading(false);
                }
            }
        }

        private void Select(BreedRowPresenter row)
        {
            if (_session == null)
            {
                return;
            }

            CancelSelection();

            _popup.Hide();
            _selection = CancellationTokenSource.CreateLinkedTokenSource(_session.Token);

            LoadDetailAsync(row, _selection.Token).Forget();
        }

        private void CancelSelection(bool resetRows = true)
        {
            if (_selection != null)
            {
                _selection.Cancel();
                _selection.Dispose();

                _selection = null;
            }

            if (resetRows)
            {
                foreach (var row in _rows)
                {
                    row.SetLoading(false);
                }
            }
        }

        private async UniTaskVoid LoadDetailAsync(BreedRowPresenter row, CancellationToken token)
        {
            row.SetLoading(true);

            try
            {
                var breed = await _api.GetBreedAsync(row.Id, token);
                token.ThrowIfCancellationRequested();

                _popup.Show(breed);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                if (!token.IsCancellationRequested)
                {
                    Debug.LogWarning("Breed detail request: " + exception.Message);

                    _view.ShowError("Описание не загрузилось. Нажмите на породу ещё раз.");
                }
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    row.SetLoading(false);
                }
            }
        }

        public void Dispose()
        {
            EndSession();

            _retrySubscription.Dispose();
            _rowSubscriptions.Dispose();
        }
    }
}
