using System;

namespace TT.Cronjobs
{
    public class CronjobInfo
    {
        public CronjobInfo(string name, string cron)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Cron = cron ?? throw new ArgumentNullException(nameof(cron));
        }

        public Type Type { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Cron { get; set; }
    }
}