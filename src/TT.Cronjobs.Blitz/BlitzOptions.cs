namespace TT.Cronjobs.Blitz
{
    public class BlitzOptions
    {
        public string ApiBaseUrl { get; set; }
        public int RetryCount { get; set; } = 5;
        public int WaitSeconds { get; set; } = 10;
        public int TimeoutSeconds { get; set; } = 60;
    }
}