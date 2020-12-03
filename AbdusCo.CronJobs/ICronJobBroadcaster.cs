using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AbdusCo.CronJobs
{
    public interface ICronJobBroadcaster
    {
        Task BroadcastAsync(IEnumerable<CronJobDescription> jobs, CancellationToken cancellationToken);
    }

    public class CronJobBroadcast
    {
        public string Application { get; set; }
        public string Environment { get; set; }
        public Guid Build { get; set; }
        public IEnumerable<CronJobDescription> Jobs { get; set; }
    }
}