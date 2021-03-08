using System;
using System.Collections.Generic;

namespace TT.Cronjobs
{
    public class HttpCronjob
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Cron { get; set; }
        public string Url { get; set; }
        public string HttpMethod { get; set; }
    }
}