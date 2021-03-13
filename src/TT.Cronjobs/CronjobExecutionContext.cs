using System;

namespace TT.Cronjobs
{
    public class CronjobExecutionContext
    {
        public Guid Id { get; }
        public ICronjob Cronjob { get; }

        public CronjobExecutionContext(Guid id, ICronjob cronjob)
        {
            Id = id;
            Cronjob = cronjob;
        }
    }
}