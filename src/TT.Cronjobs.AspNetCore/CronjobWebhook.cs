namespace TT.Cronjobs.AspNetCore
{
    public class CronjobWebhook
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string HttpMethod { get; set; }
        public string Cron { get; set; }
    }
}