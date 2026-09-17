using System;
using System.Globalization;
using Clicker.Presentation;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Views
{
    public sealed class ShellView : MonoBehaviour, IShellView
    {
        [SerializeField] private Button[] _tabButtons;
        [SerializeField] private GameObject[] _pages;
        [SerializeField] private Image[] _tabBackgrounds;
        [SerializeField] private TMP_Text[] _tabLabels;
        [SerializeField] private Image[] _tabIcons;
        [SerializeField] private TMP_Text _currencyLabel;
        [SerializeField] private TMP_Text _energyLabel;
        [SerializeField] private Color _selectedTabColor;
        [SerializeField] private Color _selectedContentColor;
        [SerializeField] private Color _unselectedContentColor;

        public IObservable<AppTab> TabSelected => Observable.Merge(
            _tabButtons[0].OnClickAsObservable().Select(_ => AppTab.Clicker),
            _tabButtons[1].OnClickAsObservable().Select(_ => AppTab.Weather),
            _tabButtons[2].OnClickAsObservable().Select(_ => AppTab.Breeds));

        public void SelectTab(AppTab tab)
        {
            for (var i = 0; i < _pages.Length; i++)
            {
                var selected = i == (int)tab;

                _pages[i].SetActive(selected);
                _tabBackgrounds[i].color = selected ? _selectedTabColor : Color.clear;
                _tabLabels[i].color = _tabIcons[i].color = selected ? _selectedContentColor : _unselectedContentColor;
            }
        }

        public void SetBalance(long currency, int energy, int maximum)
        {
            _currencyLabel.text = currency.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');
            _energyLabel.text = $"{energy} <color=#91A0BA>/ {maximum}</color>";
        }
    }
}
