namespace TT.Cronjobs.AspNetCore
{
    public class CronjobsOptions
    {
        public const string Key = "Cronjobs";
        public string RoutePattern { get; set; } = "/-/cronjobs";
        public string WebhookBaseUrl { get; set; }
        public bool IsAuthenticated { get; set; } = true;
    }
}