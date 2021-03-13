namespace TT.Cronjobs
{
    public class CronjobsOptions
    {
        public const string Key = "Cronjobs";
        public string UrlTemplate { get; set; } = "/-/cronjobs/{name}";
        public string PublicUrl { get; set; }
        public CronjobExecutionEvents Events { get; set; } = new CronjobExecutionEvents();
    }
}