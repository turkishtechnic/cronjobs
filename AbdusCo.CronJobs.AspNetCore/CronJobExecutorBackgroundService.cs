using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AbdusCo.CronJobs.AspNetCore
{
    public class CronJobExecutorBackgroundService : BackgroundService
    {
        private readonly ICronJobQueue _jobQueue;
        private readonly ILogger<CronJobExecutorBackgroundService> _logger;

        public CronJobExecutorBackgroundService(ICronJobQueue jobQueue,
            ILogger<CronJobExecutorBackgroundService> logger)
        {
            _jobQueue = jobQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for a cron job to be triggered");

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
                    await job.ExecuteAsync(stoppingToken);
                    _logger.LogInformation("Finished executing {Job}", job);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to execute {Job}", job);
                }
            }
        }
    }
}