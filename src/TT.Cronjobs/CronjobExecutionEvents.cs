using System;
using System.Threading.Tasks;

namespace TT.Cronjobs
{
    public class CronjobExecutionEvents
    {
        public Func<CronjobExecutionContext, Task> OnStarted { get; set; } = execution => Task.CompletedTask;
        public Func<CronjobExecutionContext, Task> OnFinished { get; set; } = execution => Task.CompletedTask;

        public Func<CronjobExecutionContext, Exception, Task> OnFailed { get; set; } =
            (execution, exception) => Task.CompletedTask;

        public virtual Task StartedAsync(CronjobExecutionContext cronjobExecutionContext) => OnStarted.Invoke(cronjobExecutionContext);
        public virtual Task FinishedAsync(CronjobExecutionContext cronjobExecutionContext) => OnFinished.Invoke(cronjobExecutionContext);

        public virtual Task FailedAsync(CronjobExecutionContext cronjobExecutionContext, Exception exception) =>
            OnFailed.Invoke(cronjobExecutionContext, exception);
    }
}