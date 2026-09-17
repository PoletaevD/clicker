using System;
using System.Threading;
using Clicker.Configuration;
using Clicker.Infrastructure;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Clicker.Presentation
{
    public sealed class WeatherPresenter : IDisposable
    {
        private readonly IWeatherView _view;
        private readonly IWeatherApi _api;
        private readonly GameSettings _settings;
        private CancellationTokenSource _session;
        private CancellationTokenSource _iconRequest;
        private string _loadingIconUrl;
        private string _iconUrl;
        private Sprite _icon;
        private int _pending;

        public WeatherPresenter(IWeatherView view, IWeatherApi api, GameSettings settings)
        {
            _view = view;
            _api = api;
            _settings = settings;
        }

        public void Activate()
        {
            Deactivate();

            _view.SetVisible(true);
            _session = new CancellationTokenSource();

            PollAsync(_session.Token).Forget();
        }

        public void Deactivate()
        {
            StopSession();

            _view.SetLoading(false);
            _view.SetVisible(false);
        }

        private void StopSession()
        {
            CancelIconRequest();

            if (_session != null)
            {
                _session.Cancel();
                _session.Dispose();

                _session = null;
            }

            _pending = 0;
        }

        private async UniTaskVoid PollAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    RefreshAsync(token).Forget();

                    await UniTask.Delay(TimeSpan.FromSeconds(_settings.WeatherRefreshInterval), ignoreTimeScale: true, cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTaskVoid RefreshAsync(CancellationToken token)
        {
            _pending++;
            _view.SetLoading(true);

            try
            {
                var forecast = await _api.GetForecastAsync(token);
                token.ThrowIfCancellationRequested();

                _view.ShowForecast(forecast);

                UpdateIcon(forecast.Icon, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                if (!token.IsCancellationRequested)
                {
                    Debug.LogWarning("Weather request: " + exception.Message);

                    _view.ShowError("Не удалось обновить погоду. Повторим автоматически.");
                }
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    _view.SetLoading(--_pending > 0);
                }
            }
        }

        private void UpdateIcon(string iconUrl, CancellationToken sessionToken)
        {
            if (string.IsNullOrWhiteSpace(iconUrl))
            {
                CancelIconRequest();
                ReleaseIcon();

                _view.SetIcon(null);

                return;
            }

            if (iconUrl == _iconUrl || iconUrl == _loadingIconUrl)
            {
                return;
            }

            CancelIconRequest();
            ReleaseIcon();

            _view.SetIcon(null);

            _loadingIconUrl = iconUrl;

            var requestSource = CancellationTokenSource.CreateLinkedTokenSource(sessionToken);
            _iconRequest = requestSource;

            LoadIconAsync(iconUrl, requestSource).Forget();
        }

        private async UniTaskVoid LoadIconAsync(string iconUrl, CancellationTokenSource requestSource)
        {
            Texture2D texture = null;

            try
            {
                texture = await _api.GetIconAsync(iconUrl, requestSource.Token);
                requestSource.Token.ThrowIfCancellationRequested();

                if (_iconRequest != requestSource || iconUrl != _loadingIconUrl)
                {
                    return;
                }

                var icon = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                _icon = icon;
                _iconUrl = iconUrl;

                texture = null;

                _view.SetIcon(icon);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                if (!requestSource.IsCancellationRequested)
                {
                    Debug.LogWarning("Weather icon request: " + exception.Message);
                }
            }
            finally
            {
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }

                if (_iconRequest == requestSource)
                {
                    _iconRequest = null;
                    _loadingIconUrl = null;
                }

                requestSource.Dispose();
            }
        }

        private void CancelIconRequest()
        {
            if (_iconRequest == null)
            {
                return;
            }

            var request = _iconRequest;

            _iconRequest = null;
            _loadingIconUrl = null;

            request.Cancel();
        }

        private void ReleaseIcon()
        {
            if (_icon == null)
            {
                _iconUrl = null;

                return;
            }

            UnityEngine.Object.Destroy(_icon.texture);
            UnityEngine.Object.Destroy(_icon);

            _icon = null;
            _iconUrl = null;
        }

        public void Dispose()
        {
            StopSession();
            ReleaseIcon();
        }
    }
}
