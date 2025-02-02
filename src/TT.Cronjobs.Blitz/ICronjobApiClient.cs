﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TT.Cronjobs.AspNetCore;

namespace TT.Cronjobs.Blitz
{
    public interface ICronjobApiClient
    {
        Task BatchRegisterProjectAsync(ProjectBatchRegistration registration,
                                       CancellationToken cancellationToken = default);

        Task UpdateExecutionStatusAsync(string executionId,
                                        StatusUpdate update,
                                        CancellationToken cancellationToken = default);
    }

    public sealed class StatusUpdate
    {
        public string State { get; }
        public Dictionary<string, object> Details { get; }

        public StatusUpdate(ExecutionState state, Dictionary<string, object> details = null)
        {
            State = state.Name;
            Details = details;
        }
    }

    public sealed class ProjectBatchRegistration
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public IEnumerable<CronjobWebhook> Cronjobs { get; set; }
        public string TemplateKey { get; set; }
        public TokenAuth Auth { get; set; }
    }
    
    public class TokenAuth
    {
        public string TokenEndpoint { get; set; }
        public string Scope { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public sealed class ExecutionState
    {
        public string Name { get; }

        public static readonly ExecutionState Unknown = new ExecutionState(nameof(Unknown));
        public static readonly ExecutionState Pending = new ExecutionState(nameof(Pending));
        public static readonly ExecutionState Triggered = new ExecutionState(nameof(Triggered));
        public static readonly ExecutionState Started = new ExecutionState(nameof(Started));
        public static readonly ExecutionState Finished = new ExecutionState(nameof(Finished));
        public static readonly ExecutionState Failed = new ExecutionState(nameof(Failed));
        public static readonly ExecutionState TimedOut = new ExecutionState(nameof(TimedOut));


        private ExecutionState(string name)
        {
            Name = name;
        }
    }
}