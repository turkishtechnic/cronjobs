using System;

namespace TT.Cronjobs
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CronAttribute : Attribute
    {
        public string Cron { get; }

        public CronAttribute(string cron)
        {
            if (cron == null)
            {
                throw new ArgumentNullException(nameof(cron));
            }

            if (!IsValid(cron))
            {
                throw new FormatException("Cron expression is not formatted correctly");
            }

            Cron = cron;
        }

        private bool IsValid(string cron) => cron.Split(" ").Length == 5;
    }
}