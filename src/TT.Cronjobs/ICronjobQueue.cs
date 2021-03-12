using System;
using System.Threading;
using System.Threading.Tasks;

namespace TT.Cronjobs
{
    public interface ICronjobQueue
    {
        Task EnqueueAsync(CronJobExecution job);
        Task<CronJobExecution> DequeueAsync(CancellationToken cancellationToken);
    }

    public class CronJobExecution
    {
        public Guid Id { get; }
        public ICronjob Cronjob { get; }

        public CronJobExecution(Guid id, ICronjob cronjob)
        {
            Id = id;
            Cronjob = cronjob;
        }
    }
}