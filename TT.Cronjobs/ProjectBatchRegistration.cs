using System.Collections.Generic;

namespace TT.Cronjobs
{
    public class ProjectBatchRegistration
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public List<HttpCronjob> Cronjobs { get; set; }
    }
}