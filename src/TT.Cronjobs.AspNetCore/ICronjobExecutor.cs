using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TT.Cronjobs.AspNetCore
{
    public interface ICronjobExecutor
    {
        Task ExecuteAsync(CronjobExecutionContext executionContext, CancellationToken cancellationToken = default);
    }

    class CronjobExecutor : ICronjobExecutor
    {
        private readonly ICronjobExecutionMonitor _executionMonitor;
        private readonly ILogger<CronjobExecutor> _logger;

        public CronjobExecutor(ICronjobExecutionMonitor executionMonitor, ILogger<CronjobExecutor> logger)
        {
            _executionMonitor = executionMonitor;
            _logger = logger;
        }

        public async Task ExecuteAsync(CronjobExecutionContext executionContext, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing {Cronjob}", executionContext.Cronjob);
            try
            {
                await _executionMonitor.StartedAsync(executionContext).ConfigureAwait(false);
                await executionContext.Cronjob.ExecuteAsync(cancellationToken);
                await _executionMonitor.FinishedAsync(executionContext).ConfigureAwait(false);

                _logger.LogInformation("Finished executing {Cronjob}", executionContext.Cronjob);
            }
            catch (Exception e)
            {
                await _executionMonitor.FailedAsync(executionContext, e).ConfigureAwait(false);
                _logger.LogError(e, "Failed to execute {Cronjob}", executionContext.Cronjob);
            }
            finally
            {
                _logger.LogInformation("Finished {Cronjob}", executionContext.Cronjob);
            }
        }
    }
}