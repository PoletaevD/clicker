using System;
using System.Threading;
using Clicker.Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Clicker.Infrastructure
{
    public interface IHttpTransport
    {
        UniTask<string> GetAsync(string url, CancellationToken cancellationToken);
        UniTask<Texture2D> GetTextureAsync(string url, CancellationToken cancellationToken);
    }

    public sealed class UnityHttpTransport : IHttpTransport
    {
        private readonly GameSettings _settings;

        public UnityHttpTransport(GameSettings settings)
        {
            _settings = settings;
        }

        public async UniTask<string> GetAsync(string url, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = _settings.RequestTimeoutSeconds;

                try
                {
                    await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        throw new InvalidOperationException($"HTTP {request.responseCode}: {request.error}");
                    }

                    return request.downloadHandler.text;
                }
                finally
                {
                    if (!request.isDone)
                    {
                        request.Abort();
                    }
                }
            }
        }

        public async UniTask<Texture2D> GetTextureAsync(string url, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var request = UnityWebRequestTexture.GetTexture(url, true))
            {
                request.timeout = _settings.RequestTimeoutSeconds;

                try
                {
                    await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        throw new InvalidOperationException($"HTTP {request.responseCode}: {request.error}");
                    }

                    return DownloadHandlerTexture.GetContent(request);
                }
                finally
                {
                    if (!request.isDone)
                    {
                        request.Abort();
                    }
                }
            }
        }
    }
}
