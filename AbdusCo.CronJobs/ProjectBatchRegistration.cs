using System;
using System.Collections.Generic;

namespace AbdusCo.CronJobs
{
    public class ProjectBatchRegistration
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public List<HttpCronjob> Cronjobs { get; set; }
    }
}