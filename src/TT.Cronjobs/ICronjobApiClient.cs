using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.SmartEnum;

namespace TT.Cronjobs
{
    public interface ICronjobApiClient
    {
        Task BatchRegisterProjectAsync(ProjectBatchRegistration registration,
                                       CancellationToken cancellationToken = default);

        Task UpdateExecutionStatusAsync(Guid executionId,
                                        ExecutionState state,
                                        Dictionary<string, object> details = null,
                                        CancellationToken cancellationToken = default);
    }

    public class ExecutionState : SmartEnum<ExecutionState, int>
    {
        public static readonly ExecutionState Unknown = new ExecutionState(nameof(Unknown).ToLowerInvariant(), -1);
        public static readonly ExecutionState Pending = new ExecutionState(nameof(Pending).ToLowerInvariant(), 0);
        public static readonly ExecutionState Triggered = new ExecutionState(nameof(Triggered).ToLowerInvariant(), 10);
        public static readonly ExecutionState Started = new ExecutionState(nameof(Started).ToLowerInvariant(), 20);
        public static readonly ExecutionState Finished = new ExecutionState(nameof(Finished).ToLowerInvariant(), 30);
        public static readonly ExecutionState Failed = new ExecutionState(nameof(Failed).ToLowerInvariant(), 40);
        public static readonly ExecutionState TimedOut = new ExecutionState(nameof(TimedOut).ToLowerInvariant(), 50);

        private ExecutionState(string name, int value) : base(name, value)
        {
        }
    }
}