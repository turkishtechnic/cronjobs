using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TT.Cronjobs.AspNetCore
{
    internal class CronjobExecutorBackgroundService : BackgroundService
    {
        private readonly ICronjobQueue _jobQueue;
        private readonly ILogger<CronjobExecutorBackgroundService> _logger;
        private readonly ICronjobExecutionMonitor _executionMonitor;

        public CronjobExecutorBackgroundService(ICronjobQueue jobQueue,
                                                ILogger<CronjobExecutorBackgroundService> logger,
                                                ICronjobExecutionMonitor executionMonitor)
        {
            _jobQueue = jobQueue;
            _logger = logger;
            _executionMonitor = executionMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for a cronjob to be triggered");

            while (!stoppingToken.IsCancellationRequested)
            {
                var execution = await _jobQueue.DequeueAsync(stoppingToken);
                if (execution == null)
                {
                    continue;
                }

                _logger.LogInformation("Executing {Cronjob}", execution.Cronjob);
                try
                {
                    await _executionMonitor.StartedAsync(execution).ConfigureAwait(false);
                    await execution.Cronjob.ExecuteAsync(stoppingToken);
                    await _executionMonitor.FinishedAsync(execution).ConfigureAwait(false);

                    _logger.LogInformation("Finished executing {Cronjob}", execution.Cronjob);
                }
                catch (Exception e)
                {
                    await _executionMonitor.FailedAsync(execution, e).ConfigureAwait(false);
                    _logger.LogError(e, "Failed to execute {Cronjob}", execution.Cronjob);
                }
                finally
                {
                    _logger.LogInformation("Finished {Cronjob}", execution.Cronjob);
                }
            }
        }
    }
}