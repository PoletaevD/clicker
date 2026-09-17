using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Clicker.Infrastructure
{
    public interface IRequestQueue
    {
        UniTask<T> Enqueue<T>(Func<CancellationToken, UniTask<T>> operation, CancellationToken cancellationToken);
    }

    public sealed class SerialRequestQueue : IRequestQueue, IDisposable
    {
        private readonly object _gate = new object();
        private readonly LinkedList<IJob> _pending = new LinkedList<IJob>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private bool _running;
        private bool _disposed;

        public int PendingCount
        {
            get
            {
                lock (_gate)
                {
                    return _pending.Count;
                }
            }
        }

        public UniTask<T> Enqueue<T>(Func<CancellationToken, UniTask<T>> operation, CancellationToken cancellationToken)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SerialRequestQueue));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return UniTask.FromCanceled<T>(cancellationToken);
            }

            var job = new Job<T>(operation, cancellationToken, _lifetime.Token);
            lock (_gate)
            {
                job.Node = _pending.AddLast(job);
            }

            job.RegisterCancellation(() => RemovePending(job));
            if (!_running)
            {
                _running = true;
                PumpAsync().Forget();
            }

            return job.Task;
        }

        private void RemovePending(IJob job)
        {
            lock (_gate)
            {
                if (job.Node == null)
                {
                    return;
                }

                _pending.Remove(job.Node);
                job.Node = null;
            }

            job.CancelPending();
        }

        private async UniTaskVoid PumpAsync()
        {
            while (true)
            {
                IJob job;
                lock (_gate)
                {
                    if (_pending.Count == 0)
                    {
                        _running = false;
                        return;
                    }

                    job = _pending.First.Value;
                    _pending.RemoveFirst();
                    job.Node = null;
                }

                await job.RunAsync();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private interface IJob
        {
            LinkedListNode<IJob> Node { get; set; }

            UniTask RunAsync();
            void CancelPending();
        }

        private sealed class Job<T> : IJob
        {
            private readonly Func<CancellationToken, UniTask<T>> _operation;
            private readonly CancellationTokenSource _cancellation;
            private readonly CancellationToken _token;
            private readonly UniTaskCompletionSource<T> _completion = new UniTaskCompletionSource<T>();
            private CancellationTokenRegistration _registration;
            private int _cleaned;

            public LinkedListNode<IJob> Node { get; set; }
            public UniTask<T> Task => _completion.Task;

            public Job(Func<CancellationToken, UniTask<T>> operation, CancellationToken caller,
                CancellationToken lifetime)
            {
                _operation = operation;
                _cancellation = CancellationTokenSource.CreateLinkedTokenSource(caller, lifetime);
                _token = _cancellation.Token;
            }

            public void RegisterCancellation(Action action)
            {
                _registration = _token.Register(action);
                if (Volatile.Read(ref _cleaned) != 0)
                {
                    _registration.Dispose();
                }
            }

            public void CancelPending()
            {
                _completion.TrySetCanceled(_token);
                Cleanup();
            }

            public async UniTask RunAsync()
            {
                try
                {
                    _token.ThrowIfCancellationRequested();
                    var result = await _operation(_token);
                    _token.ThrowIfCancellationRequested();
                    _completion.TrySetResult(result);
                }
                catch (OperationCanceledException)
                {
                    _completion.TrySetCanceled(_token);
                }
                catch (Exception exception)
                {
                    _completion.TrySetException(exception);
                }
                finally
                {
                    Cleanup();
                }
            }

            private void Cleanup()
            {
                if (Interlocked.Exchange(ref _cleaned, 1) != 0)
                {
                    return;
                }

                _registration.Dispose();
                _cancellation.Dispose();
            }
        }
    }
}
