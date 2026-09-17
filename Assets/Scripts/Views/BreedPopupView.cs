using Clicker.Domain;
using Clicker.Presentation;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clicker.Views
{
    public sealed class BreedPopupView : MonoBehaviour, IBreedPopupView
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _backdrop;
        [SerializeField] private CanvasGroup _group;

        private Tween _fade;

        private void Awake()
        {
            _title.richText = false;
            _description.richText = false;

            _closeButton.OnClickAsObservable().Subscribe(_ => Hide()).AddTo(this);
            _confirmButton.OnClickAsObservable().Subscribe(_ => Hide()).AddTo(this);
            _backdrop.OnClickAsObservable().Subscribe(_ => Hide()).AddTo(this);
        }

        public void Show(Breed breed)
        {
            _title.text = breed.Name;
            _description.text = breed.Description;

            gameObject.SetActive(true);

            _fade?.Kill();

            _group.alpha = 0;

            _fade = DOTween.To(() => _group.alpha, a => _group.alpha = a, 1, .18f).SetUpdate(true);
        }

        public void Hide()
        {
            _fade?.Kill();

            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            var keyboard = Keyboard.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Hide();
            }
        }

        private void OnDisable()
        {
            _fade?.Kill();
        }
    }
}
