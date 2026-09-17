using System;
using Clicker.Domain;
using Clicker.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Views
{
    public sealed class WeatherView : MonoBehaviour, IWeatherView
    {
        [SerializeField] private GameObject _loader;
        [SerializeField] private GameObject _forecastCard;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private TMP_Text _temperature;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Image _weatherIcon;

        private bool _hasForecast;
        private bool _hasError;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetLoading(bool loading)
        {
            _loader.SetActive(loading);

            if (loading && !_hasForecast && !_hasError)
            {
                _status.text = "Получаем прогноз…";
            }
        }

        public void ShowForecast(WeatherForecast forecast)
        {
            _hasForecast = true;
            _hasError = false;

            _forecastCard.SetActive(true);
            _emptyState.SetActive(false);

            _temperature.text = $"Сегодня — {forecast.Temperature}°{forecast.Unit}";
            _status.text = "Обновлено в " + DateTime.Now.ToString("HH:mm:ss");
        }

        public void SetIcon(Sprite icon)
        {
            _weatherIcon.sprite = icon;
            _weatherIcon.color = Color.white;
            _weatherIcon.enabled = icon != null;
        }

        public void ShowError(string message)
        {
            _hasError = true;

            _status.text = message + (_hasForecast ? " Показан последний прогноз." : "");

            _emptyState.SetActive(!_hasForecast);
        }
    }
}
