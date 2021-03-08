namespace AbdusCo.CronJobs
{
    public class CronjobsOptions
    {
        public const string Key = "Cronjobs";
        public string RegistrationApiUrl { get; set; }
        public string UrlTemplate { get; set; }
        public int RetryCount { get; set; } = 5;
        public int WaitSeconds { get; set; } = 5;
        public int TimeoutSeconds { get; set; } = 60;
    }
}