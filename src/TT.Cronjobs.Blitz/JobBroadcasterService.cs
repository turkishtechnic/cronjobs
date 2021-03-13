using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TT.Cronjobs.AspNetCore;

namespace TT.Cronjobs.Blitz
{
    internal class JobBroadcasterService : BackgroundService
    {
        private readonly BlitzOptions _options;
        private readonly CronjobWebhookProvider _cronjobWebhookProvider;
        private readonly ICronjobBroadcaster _broadcaster;
        private readonly ILogger<JobBroadcasterService> _logger;

        public JobBroadcasterService(
            IOptions<BlitzOptions> options,
            ICronjobBroadcaster broadcaster,
            ILogger<JobBroadcasterService> logger,
            CronjobWebhookProvider cronjobWebhookProvider)
        {
            _options = options.Value;
            _broadcaster = broadcaster;
            _logger = logger;
            _cronjobWebhookProvider = cronjobWebhookProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(_options.WaitSeconds), stoppingToken);

            _logger.LogInformation("Finding cronjobs");
            List<CronjobWebhook> jobs;
            try
            {
                jobs = _cronjobWebhookProvider.Cronjobs.ToList();
                _logger.LogInformation($"Found {jobs.Count} cronjobs. Broadcasting all cronjobs");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while discovering cronjobs");
                throw;
            }

            try
            {
                await _broadcaster.BroadcastAsync(jobs, stoppingToken);
                _logger.LogInformation("Jobs have been broadcast successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to broadcast cronjobs.");
            }
        }
    }
}