using System;
using Clicker.Domain;
using UnityEngine;

namespace Clicker.Presentation
{
    public enum AppTab
    {
        Clicker,
        Weather,
        Breeds
    }

    public interface IShellView
    {
        IObservable<AppTab> TabSelected { get; }

        void SelectTab(AppTab tab);
        void SetBalance(long currency, int energy, int maximum);
    }

    public interface IClickerView
    {
        IObservable<UniRx.Unit> Clicked { get; }

        void SetEconomy(int reward, int cost, int recharge, float rechargeSeconds, float autoSeconds);
        void SetEnergy(int energy, int maximum, bool canCollect);
        void SetTimers(float autoProgress, float rechargeProgress);
        void PlayReward(ClickReward reward);
        void ShowInsufficientEnergy();
    }

    public interface IWeatherView
    {
        void SetVisible(bool visible);
        void SetLoading(bool loading);
        void ShowForecast(WeatherForecast forecast);
        void SetIcon(Sprite icon);
        void ShowError(string message);
    }

    public interface IBreedsView
    {
        IObservable<UniRx.Unit> RetryClicked { get; }

        void SetVisible(bool visible);
        void SetLoading(bool loading);
        void ShowError(string message);
        void ShowCount(int count);
    }

    public interface IBreedPopupView
    {
        void Show(Breed breed);
        void Hide();
    }
}
