using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AbdusCo.CronJobs.AspNetCore
{
    internal class JobBroadcasterService : BackgroundService
    {
        private readonly CronJobsOptions _options;
        private readonly IEnumerable<ICronJobProvider> _jobProviders;
        private readonly ICronJobBroadcaster _broadcaster;
        private readonly ILogger<JobBroadcasterService> _logger;

        public JobBroadcasterService(
            IOptions<CronJobsOptions> options,
            IEnumerable<ICronJobProvider> jobProviders,
            ICronJobBroadcaster broadcaster, ILogger<JobBroadcasterService> logger)
        {
            _options = options.Value;
            _jobProviders = jobProviders;
            _broadcaster = broadcaster;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(_options.WaitSeconds), stoppingToken);
            
            _logger.LogInformation("Finding jobs");
            List<CronJobDescription> jobs;
            try
            {
                jobs = _jobProviders.SelectMany(p => p.CronJobs).ToList();
                _logger.LogInformation($"Found {jobs.Count} jobs. Broadcasting all jobs");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error happened while listing of jobs");
                throw;
            }

            try
            {
                await _broadcaster.BroadcastAsync(jobs, stoppingToken);
                _logger.LogInformation("Jobs have been broadcast successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Cannot broadcast jobs.");
            }
        }
    }
}