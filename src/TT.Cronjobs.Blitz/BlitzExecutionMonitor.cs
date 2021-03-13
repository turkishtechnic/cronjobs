using System;
using System.Threading.Tasks;
using TT.Cronjobs.AspNetCore;

namespace TT.Cronjobs.Blitz
{
    public class BlitzExecutionMonitor : ICronjobExecutionMonitor
    {
        public Task StartedAsync(CronjobExecutionContext cronjobExecutionContext) => Task.CompletedTask;
        public Task FinishedAsync(CronjobExecutionContext cronjobExecutionContext) => Task.CompletedTask;
        public Task FailedAsync(CronjobExecutionContext cronjobExecutionContext, Exception exception) => Task.CompletedTask;
    }
}