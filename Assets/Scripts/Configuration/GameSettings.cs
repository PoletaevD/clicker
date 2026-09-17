using UnityEngine;

namespace Clicker.Configuration
{
    [CreateAssetMenu(menuName = "Clicker/Game settings")]
    public sealed class GameSettings : ScriptableObject
    {
        [Header("Economy")]
        [SerializeField, Min(0)] private long _initialCurrency;
        [SerializeField, Min(1)] private int _currencyPerClick = 1;
        [SerializeField, Min(1)] private int _energyPerClick = 1;
        [SerializeField, Min(1)] private int _maximumEnergy = 1000;
        [SerializeField, Min(0)] private int _initialEnergy = 1000;
        [SerializeField, Min(1)] private int _energyPerRecharge = 10;
        [SerializeField, Min(0.1f)] private float _rechargeInterval = 10f;
        [SerializeField, Min(0.1f)] private float _autoClickInterval = 3f;

        [Header("Server requests")]
        [SerializeField] private string _weatherUrl = "https://api.weather.gov/gridpoints/TOP/32,81/forecast";
        [SerializeField] private string _breedsUrl = "https://dogapi.dog/api/v2/breeds";
        [SerializeField, Min(1)] private int _requestTimeoutSeconds = 20;
        [SerializeField, Min(0.1f)] private float _weatherRefreshInterval = 5f;
        [SerializeField, Range(1, 10)] private int _breedCount = 10;

        [Header("Feedback")]
        [SerializeField, Range(0f, 1f)] private float _clickVolume = 0.35f;
        [SerializeField, Range(1, 24)] private int _burstParticleCount = 10;
        [SerializeField, Min(0.1f)] private float _rewardFlightDuration = 0.85f;
        [SerializeField] private AudioClip _clickSound;

        public long InitialCurrency => _initialCurrency;
        public int CurrencyPerClick => _currencyPerClick;
        public int EnergyPerClick => _energyPerClick;
        public int MaximumEnergy => _maximumEnergy;
        public int InitialEnergy => _initialEnergy;
        public int EnergyPerRecharge => _energyPerRecharge;
        public float RechargeInterval => _rechargeInterval;
        public float AutoClickInterval => _autoClickInterval;
        public string WeatherUrl => _weatherUrl;
        public string BreedsUrl => _breedsUrl;
        public int RequestTimeoutSeconds => _requestTimeoutSeconds;
        public float WeatherRefreshInterval => _weatherRefreshInterval;
        public int BreedCount => _breedCount;
        public float ClickVolume => _clickVolume;
        public int BurstParticleCount => _burstParticleCount;
        public float RewardFlightDuration => _rewardFlightDuration;
        public AudioClip ClickSound => _clickSound;

        private void OnValidate()
        {
            _initialCurrency = System.Math.Max(0, _initialCurrency);
            _currencyPerClick = Mathf.Max(1, _currencyPerClick);
            _energyPerClick = Mathf.Max(1, _energyPerClick);
            _maximumEnergy = Mathf.Max(1, _maximumEnergy);
            _initialEnergy = Mathf.Clamp(_initialEnergy, 0, _maximumEnergy);
            _energyPerRecharge = Mathf.Max(1, _energyPerRecharge);
            _rechargeInterval = Mathf.Max(0.1f, _rechargeInterval);
            _autoClickInterval = Mathf.Max(0.1f, _autoClickInterval);
            _weatherRefreshInterval = Mathf.Max(0.1f, _weatherRefreshInterval);
            _requestTimeoutSeconds = Mathf.Max(1, _requestTimeoutSeconds);
            _breedCount = Mathf.Clamp(_breedCount, 1, 10);
            _rewardFlightDuration = Mathf.Max(0.1f, _rewardFlightDuration);
        }
    }
}
