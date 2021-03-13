using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TT.Cronjobs.AspNetCore
{
    internal class CronjobExecutorBackgroundService : BackgroundService
    {
        private readonly ICronjobQueue _jobQueue;
        private readonly CronjobsOptions _options;
        private readonly ILogger<CronjobExecutorBackgroundService> _logger;

        public CronjobExecutorBackgroundService(ICronjobQueue jobQueue,
                                                ILogger<CronjobExecutorBackgroundService> logger,
                                                IOptions<CronjobsOptions> options)
        {
            _jobQueue = jobQueue;
            _logger = logger;
            _options = options.Value;
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
                    await _options.Events.StartedAsync(execution).ConfigureAwait(false);
                    await execution.Cronjob.ExecuteAsync(stoppingToken);
                    await _options.Events.FinishedAsync(execution).ConfigureAwait(false);

                    _logger.LogInformation("Finished executing {Cronjob}", execution.Cronjob);
                }
                catch (Exception e)
                {
                    await _options.Events.FailedAsync(execution, e).ConfigureAwait(false);
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