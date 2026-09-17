using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Clicker.Views
{
    public sealed class BreedRowView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _number;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private GameObject _loader;

        public IObservable<Unit> Clicked => _button.OnClickAsObservable();

        private void Awake()
        {
            _nameLabel.richText = false;
        }

        public void Render(int index, string breedName)
        {
            _number.text = index.ToString("00") + " −";
            _nameLabel.text = breedName;

            SetLoading(false);
        }

        public void SetLoading(bool loading)
        {
            _loader.SetActive(loading);
        }

        public sealed class Factory : PlaceholderFactory<BreedRowView>
        {
        }
    }
}
