using System;

namespace TT.Cronjobs
{
    public class CronjobExecutionContext
    {
        public IServiceProvider Services { get; set; }
        public string ExecutionId { get; }
        public ICronjob Cronjob { get; }

        public CronjobExecutionContext(ICronjob cronjob, string executionId, IServiceProvider services)
        {
            ExecutionId = executionId;
            Services = services;
            Cronjob = cronjob;
        }
    }
}