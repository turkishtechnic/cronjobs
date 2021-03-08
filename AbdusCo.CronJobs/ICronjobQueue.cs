using System.Threading;
using System.Threading.Tasks;

namespace AbdusCo.CronJobs
{
    public interface ICronjobQueue
    {
        Task EnqueueAsync(CronJobExecution job);
        Task<CronJobExecution> DequeueAsync(CancellationToken cancellationToken);
    }

    public class CronJobExecution
    {
        public string Id { get; }
        public ICronjob Cronjob { get; }

        public CronJobExecution(string id, ICronjob cronjob)
        {
            Id = id;
            Cronjob = cronjob;
        }
    }
}