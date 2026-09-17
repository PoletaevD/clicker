using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Clicker.Configuration;
using Clicker.Domain;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Clicker.Infrastructure
{
    public interface IWeatherApi
    {
        UniTask<WeatherForecast> GetForecastAsync(CancellationToken token);
        UniTask<Texture2D> GetIconAsync(string url, CancellationToken token);
    }

    public interface IBreedsApi
    {
        UniTask<IReadOnlyList<Breed>> GetBreedsAsync(CancellationToken token);
        UniTask<Breed> GetBreedAsync(string id, CancellationToken token);
    }

    public sealed class ApiClient : IWeatherApi, IBreedsApi
    {
        private readonly IRequestQueue _queue;
        private readonly IHttpTransport _http;
        private readonly GameSettings _settings;
        private readonly ApiParser _parser;

        public ApiClient(IRequestQueue queue, IHttpTransport http, GameSettings settings, ApiParser parser)
        {
            _queue = queue;
            _http = http;
            _settings = settings;
            _parser = parser;
        }

        public UniTask<WeatherForecast> GetForecastAsync(CancellationToken token)
        {
            return _queue.Enqueue(async cancellation =>
                _parser.Forecast(await _http.GetAsync(_settings.WeatherUrl, cancellation)), token);
        }

        public UniTask<Texture2D> GetIconAsync(string url, CancellationToken token)
        {
            return _queue.Enqueue(cancellation => _http.GetTextureAsync(url, cancellation), token);
        }

        public UniTask<IReadOnlyList<Breed>> GetBreedsAsync(CancellationToken token)
        {
            var url = _settings.BreedsUrl.TrimEnd('/') + "?page%5Bsize%5D=" + _settings.BreedCount;

            return _queue.Enqueue(async cancellation =>
                _parser.Breeds(await _http.GetAsync(url, cancellation), _settings.BreedCount), token);
        }

        public UniTask<Breed> GetBreedAsync(string id, CancellationToken token)
        {
            var url = _settings.BreedsUrl.TrimEnd('/') + "/" + Uri.EscapeDataString(id);

            return _queue.Enqueue(async cancellation => _parser.Breed(await _http.GetAsync(url, cancellation)), token);
        }
    }

    public sealed class ApiParser
    {
        public WeatherForecast Forecast(string json)
        {
            var response = JObject.Parse(json);
            var properties = response["properties"] as JObject;
            var periods = properties?["periods"] as JArray;

            if (periods == null || periods.Count == 0 || !(periods[0] is JObject period))
            {
                throw new FormatException("The forecast response has no periods.");
            }

            return new WeatherForecast(ReadRequiredInteger(period, "temperature"),
                ReadRequiredString(period, "temperatureUnit"), ReadOptionalString(period, "shortForecast") ?? "",
                ReadOptionalString(period, "icon") ?? "");
        }

        public IReadOnlyList<Breed> Breeds(string json, int limit)
        {
            var response = JObject.Parse(json);

            if (!(response["data"] is JArray data))
            {
                throw new FormatException("The breed list is missing.");
            }

            var result = new List<Breed>();
            foreach (var item in data)
            {
                if (result.Count >= limit)
                {
                    break;
                }

                result.Add(Map(item));
            }

            return result;
        }

        public Breed Breed(string json)
        {
            return Map(JObject.Parse(json)["data"]);
        }

        private Breed Map(JToken token)
        {
            if (!(token is JObject item) || !(item["attributes"] is JObject attributes))
            {
                throw new FormatException("The breed response is incomplete.");
            }

            return new Breed(ReadRequiredString(item, "id"), ReadRequiredString(attributes, "name"),
                ReadOptionalString(attributes, "description") ?? "Описание отсутствует.");
        }

        private int ReadRequiredInteger(JObject parent, string key)
        {
            var token = parent[key];

            if (token == null || token.Type != JTokenType.Integer || !int.TryParse(token.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                throw new FormatException($"The response is missing a valid integer '{key}'.");
            }

            return value;
        }

        private string ReadRequiredString(JObject parent, string key)
        {
            var value = ReadOptionalString(parent, key);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException($"The response is missing a valid string '{key}'.");
            }

            return value;
        }

        private string ReadOptionalString(JObject parent, string key)
        {
            var token = parent[key];

            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token.Type != JTokenType.String)
            {
                throw new FormatException($"The response contains an invalid string '{key}'.");
            }

            return (string)token;
        }
    }
}
