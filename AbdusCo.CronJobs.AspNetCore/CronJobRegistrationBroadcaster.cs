using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace AbdusCo.CronJobs.AspNetCore
{
    internal class CronJobRegistrationBroadcaster : ICronJobBroadcaster
    {
        private readonly HttpClient _httpClient;
        private readonly IHostEnvironment _environment;

        public CronJobRegistrationBroadcaster(HttpClient httpClient,
            IHostEnvironment environment)
        {
            _httpClient = httpClient;
            _environment = environment;
        }

        public async Task BroadcastAsync(IEnumerable<CronJobDescription> jobs, CancellationToken cancellationToken)
        {
            var payload = new CronJobBroadcast
            {
                Application = _environment.ApplicationName,
                Environment = _environment.EnvironmentName,
                Build = Assembly.GetEntryAssembly()!.ManifestModule.ModuleVersionId,
                Jobs = jobs
            };

            var json = JsonSerializer.Serialize(payload);
            var res = await _httpClient.PostAsync(
                "",
                new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json),
                cancellationToken: cancellationToken
            );
            res.EnsureSuccessStatusCode();
        }
    }
}