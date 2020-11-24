using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace AbdusCo.CronJobs.AspNetCore
{
    public class CronJobExecutor: ICronJobExecutor
    {
        private readonly ILogger<CronJobExecutor> _logger;
        private readonly CancellationToken _token;

        public CronJobExecutor(ILogger<CronJobExecutor> logger, IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _token = lifetime.ApplicationStopping;
        }

        public async Task ExecuteJobAsync(ICronJob cronJob)
        {
            _logger.LogInformation($"Executing {cronJob}");
            await cronJob.ExecuteAsync(_token).ConfigureAwait(false);
            _logger.LogInformation($"Finished {cronJob}");
        }
    }
}