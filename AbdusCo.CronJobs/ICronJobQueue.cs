using System.Threading;
using System.Threading.Tasks;

namespace AbdusCo.CronJobs
{
    public interface ICronJobQueue
    {
        Task EnqueueAsync(ICronJob job);
        Task<ICronJob> DequeueAsync(CancellationToken cancellationToken);
    }
}