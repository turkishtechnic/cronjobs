using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AbdusCo.CronJobs
{
    public interface ICronjobBroadcaster
    {
        Task BroadcastAsync(List<HttpCronjob> jobs, CancellationToken cancellationToken);
    }
}