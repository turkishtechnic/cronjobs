using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AbdusCo.CronJobs.Hangfire
{
    public class CronJobTriggerer : ICronJobTriggerer
    {
        private readonly HttpClient _http;
        private readonly ILogger<CronJobTriggerer> _logger;

        public CronJobTriggerer(HttpClient http, ILogger<CronJobTriggerer> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task Trigger(CronJobDescription cronJob)
        {
            _logger.LogInformation("Triggering {Job}", cronJob.Name);
            try
            {
                // fire and forget
                var res = await _http.PostAsync(cronJob.Endpoint, new StringContent(""));
            }
            catch (TaskCanceledException)
            {
                // ignore timeout errors
            }

            _logger.LogInformation($"Sent request to {cronJob.Endpoint}");
        }
    }
}