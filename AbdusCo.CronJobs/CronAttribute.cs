using System;
using System.Linq;

namespace AbdusCo.CronJobs
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CronAttribute : Attribute
    {
        public string[] CronExpressions { get; }

        public CronAttribute(params string[] cronExpressions)
        {
            if (!cronExpressions.Any())
            {
                throw new ArgumentException("You must provide at least one cron expression", nameof(cronExpressions));
            }

            if (cronExpressions.Any(e => !IsValid(e)))
            {
                throw new ArgumentException("Invalid cron expression", nameof(cronExpressions));
            }

            CronExpressions = cronExpressions;
        }

        private bool IsValid(string cron) => cron.Split(" ").Length == 5;
    }
}