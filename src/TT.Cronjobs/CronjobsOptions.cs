namespace TT.Cronjobs
{
    public class CronjobsOptions
    {
        public const string Key = "Cronjobs";
        public string ApiBaseUrl { get; set; }
        public string UrlTemplate { get; set; }
        public int RetryCount { get; set; } = 5;
        public int WaitSeconds { get; set; } = 10;
        public int TimeoutSeconds { get; set; } = 60;
    }
}