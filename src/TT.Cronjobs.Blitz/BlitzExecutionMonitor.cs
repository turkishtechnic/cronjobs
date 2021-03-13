using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TT.Cronjobs.AspNetCore;

namespace TT.Cronjobs.Blitz
{
    internal class BlitzExecutionMonitor : ICronjobExecutionMonitor
    {
        private readonly ICronjobApiClient _cronjobApi;

        public BlitzExecutionMonitor(ICronjobApiClient cronjobApi) => _cronjobApi = cronjobApi;

        public Task StartedAsync(CronjobExecutionContext cronjobExecutionContext) =>
            _cronjobApi.UpdateExecutionStatusAsync(
                cronjobExecutionContext.ExecutionId,
                new StatusUpdate(ExecutionState.Started)
            );

        public Task FinishedAsync(CronjobExecutionContext cronjobExecutionContext) =>
            _cronjobApi.UpdateExecutionStatusAsync(
                cronjobExecutionContext.ExecutionId,
                new StatusUpdate(ExecutionState.Finished)
            );

        public Task FailedAsync(CronjobExecutionContext cronjobExecutionContext, Exception exception) =>
            _cronjobApi.UpdateExecutionStatusAsync(
                cronjobExecutionContext.ExecutionId,
                new StatusUpdate(ExecutionState.Failed, new Dictionary<string, object>
                {
                    ["ExceptionMessage"] = exception.Message,
                    ["ExceptionSource"] = exception.Source,
                    ["ExceptionStackTrace"] = exception.StackTrace,
                })
            );
    }
}