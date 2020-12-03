using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AbdusCo.CronJobs.AspNetCore
{
    public class SynchronizedCronJobQueue : ICronJobQueue
    {
        private readonly ConcurrentQueue<ICronJob> _jobQueue =
            new ConcurrentQueue<ICronJob>();

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public Task EnqueueAsync(ICronJob task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            _jobQueue.Enqueue(task);
            _signal.Release();
            return Task.CompletedTask;
        }

        public async Task<ICronJob> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _jobQueue.TryDequeue(out var task);
            return task;
        }
    }
}