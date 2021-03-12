using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TT.Cronjobs.AspNetCore
{
    internal class CronJobExecutorBackgroundService : BackgroundService
    {
        private readonly ICronjobQueue _jobQueue;
        private readonly ICronjobApiClient _cronjobApi;
        private readonly ILogger<CronJobExecutorBackgroundService> _logger;

        public CronJobExecutorBackgroundService(ICronjobQueue jobQueue,
                                                ILogger<CronJobExecutorBackgroundService> logger,
                                                ICronjobApiClient cronjobApi)
        {
            _jobQueue = jobQueue;
            _logger = logger;
            _cronjobApi = cronjobApi;
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

                _logger.LogInformation("Executing {Job}", execution);
                var timer = Stopwatch.StartNew();
                try
                {
                    await _cronjobApi.UpdateExecutionStatusAsync(execution.Id, ExecutionState.Started,
                        cancellationToken: stoppingToken);
                    
                    await execution.Cronjob.ExecuteAsync(stoppingToken);
                    
                    timer.Stop();
                    await _cronjobApi.UpdateExecutionStatusAsync(execution.Id, ExecutionState.Finished,
                        details: new Dictionary<string, object>
                        {
                            ["Elapsed"] = timer.ElapsedMilliseconds,
                        },
                        cancellationToken: stoppingToken);
                    
                    _logger.LogInformation("Finished executing {Job}", execution);
                }
                catch (Exception e)
                {
                    timer.Stop();
                    await _cronjobApi.UpdateExecutionStatusAsync(execution.Id, ExecutionState.Failed,
                        details: new Dictionary<string, object>
                        {
                            ["ExceptionMessage"] = e.Message,
                            ["ExceptionSource"] = e.Source,
                            ["ExceptionStackTrace"] = e.StackTrace,
                        },
                        cancellationToken: stoppingToken);
                    _logger.LogError(e, "Failed to execute {Job}", execution);
                }
                finally
                {
                    _logger.LogInformation("Finished {Job}", execution);
                }
            }
        }
    }
}