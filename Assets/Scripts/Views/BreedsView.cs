using System;
using Clicker.Presentation;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Views
{
    public sealed class BreedsView : MonoBehaviour, IBreedsView
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private GameObject _loader;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Button _retryButton;
        [SerializeField] private ScrollRect _scroll;

        public RectTransform Content => _content;
        public IObservable<Unit> RetryClicked => _retryButton.OnClickAsObservable();

        private void Awake()
        {
            _status.richText = false;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);

            if (visible)
            {
                _status.text = "";

                _retryButton.gameObject.SetActive(false);

                _scroll.verticalNormalizedPosition = 1;
            }
        }

        public void SetLoading(bool loading)
        {
            _loader.SetActive(loading);

            if (loading)
            {
                _status.text = "Загружаем породы…";

                _retryButton.gameObject.SetActive(false);
            }
        }

        public void ShowError(string message)
        {
            _status.text = message;

            _retryButton.gameObject.SetActive(true);
        }

        public void ShowCount(int count)
        {
            _status.text = count == 0 ? "Список пока пуст." : $"{count} пород  /  Нажмите, чтобы узнать больше";

            _retryButton.gameObject.SetActive(count == 0);

            Canvas.ForceUpdateCanvases();

            _scroll.verticalNormalizedPosition = 1;
        }
    }
}
