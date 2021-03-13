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
        private readonly ICronjobExecutor _executor;

        public CronjobExecutorBackgroundService(ICronjobQueue jobQueue,
                                                ILogger<CronjobExecutorBackgroundService> logger,
                                                ICronjobExecutor executor)
        {
            _jobQueue = jobQueue;
            _logger = logger;
            _executor = executor;
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

                _logger.LogInformation("Dequeued {Cronjob} with executionId={ExecutionId}", execution.GetType().Name, execution.ExecutionId);
                await _executor.ExecuteAsync(execution, stoppingToken);
            }
        }
    }
}