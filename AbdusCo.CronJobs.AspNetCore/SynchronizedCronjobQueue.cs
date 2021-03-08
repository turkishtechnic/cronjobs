using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AbdusCo.CronJobs.AspNetCore
{
    public class SynchronizedCronjobQueue : ICronjobQueue
    {
        private readonly ConcurrentQueue<CronJobExecution> _jobQueue =
            new ConcurrentQueue<CronJobExecution>();

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public Task EnqueueAsync(CronJobExecution task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            _jobQueue.Enqueue(task);
            _signal.Release();
            return Task.CompletedTask;
        }

        public async Task<CronJobExecution> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _jobQueue.TryDequeue(out var task);
            return task;
        }
    }
}