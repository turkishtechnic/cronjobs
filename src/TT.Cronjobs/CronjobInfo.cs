using System;

namespace TT.Cronjobs
{
    public class CronjobInfo
    {
        public Type Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Cron { get; set; }
    }
}