using System.Collections.Generic;

namespace AbdusCo.CronJobs
{
    public interface ICronjobProvider
    {
        IEnumerable<HttpCronjob> CronJobs { get; }
    }
}