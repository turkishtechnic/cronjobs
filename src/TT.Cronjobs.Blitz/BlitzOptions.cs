namespace TT.Cronjobs.Blitz
{
    public class BlitzOptions
    {
        public string ApiBaseUrl { get; set; }
        public int RetryCount { get; set; } = 3;
        public int WaitSeconds { get; set; } = 10;
        public int TimeoutSeconds { get; set; } = 60;
        public string TemplateKey { get; set; }
        public TokenAuth Auth { get; set; }
    }
}