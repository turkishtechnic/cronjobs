using System;
using System.Threading.Tasks;

namespace TT.Cronjobs.AspNetCore
{
    public interface ICronjobExecutionMonitor
    {
        Task StartedAsync(CronjobExecutionContext cronjobExecutionContext);
        Task FinishedAsync(CronjobExecutionContext cronjobExecutionContext);
        Task FailedAsync(CronjobExecutionContext cronjobExecutionContext, Exception exception);
    }

    class NoopCronjobExecutionMonitor : ICronjobExecutionMonitor
    {
        public Task StartedAsync(CronjobExecutionContext cronjobExecutionContext) => Task.CompletedTask;
        public Task FinishedAsync(CronjobExecutionContext cronjobExecutionContext) => Task.CompletedTask;
        public Task FailedAsync(CronjobExecutionContext cronjobExecutionContext, Exception exception) => Task.CompletedTask;
    }
}