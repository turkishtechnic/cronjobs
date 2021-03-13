using System;

namespace TT.Cronjobs
{
    public class CronjobsOptions
    {
        public const string Key = "Cronjobs";
        public string UrlTemplate { get; set; } = "/-/cronjobs/{name}";
        public string PublicUrl { get; set; }
        public string ApiBaseUrl { get; set; }
        // public int RetryCount { get; set; } = 5;
        // public int WaitSeconds { get; set; } = 10;
        // public int TimeoutSeconds { get; set; } = 60;

        public string MakeUrl(string name) => new Uri(new Uri(PublicUrl), UrlTemplate).ToString();
        public CronjobExecutionEvents Events { get; set; } = new CronjobExecutionEvents();
    }
}