using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AbdusCo.CronJobs.AspNetCore
{
    public class CronJobExecutorBackgroundService : BackgroundService
    {
        private readonly ICronjobQueue _jobQueue;
        private readonly ILogger<CronJobExecutorBackgroundService> _logger;

        public CronJobExecutorBackgroundService(ICronjobQueue jobQueue,
            ILogger<CronJobExecutorBackgroundService> logger)
        {
            _jobQueue = jobQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for a cronjob to be triggered");

            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await _jobQueue.DequeueAsync(stoppingToken);
                if (job == null)
                {
                    continue;
                }

                _logger.LogInformation("Executing {Job}", job);
                try
                {
                    // TODO: await UpdateStatusAsync(ExeStatus.Started);
                    await job.Cronjob.ExecuteAsync(stoppingToken);
                    // TODO: await UpdateStatusAsync(ExeStatus.Finished);
                    _logger.LogInformation("Finished executing {Job}", job);
                }
                catch (Exception e)
                {
                    // TODO: await UpdateStatusAsync(ExeStatus.Failed);
                    _logger.LogError(e, "Failed to execute {Job}", job);
                }
            }
        }
    }
}