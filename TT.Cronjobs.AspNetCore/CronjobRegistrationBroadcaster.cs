using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TT.Cronjobs.AspNetCore
{
    internal class CronjobRegistrationBroadcaster : ICronjobBroadcaster
    {
        private readonly HttpClient _httpClient;
        private readonly IHostEnvironment _environment;

        public CronjobRegistrationBroadcaster(HttpClient httpClient,
                                              IHostEnvironment environment)
        {
            _httpClient = httpClient;
            _environment = environment;
        }

        public async Task BroadcastAsync(List<HttpCronjob> jobs, CancellationToken cancellationToken)
        {
            var buildId = Assembly.GetEntryAssembly()!.ManifestModule.ModuleVersionId.ToString();
            var payload = new ProjectBatchRegistration
            {
                Title = $"{_environment.ApplicationName} ({_environment.EnvironmentName})",
                Version = buildId.Substring(0, Math.Min(buildId.Length, 8)),
                Cronjobs = jobs
            };

            var json = JsonSerializer.Serialize(payload);
            var res = await _httpClient.PostAsync(
                "/api/projects/batchcreate",
                new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json),
                cancellationToken: cancellationToken
            );
            res.EnsureSuccessStatusCode();
        }
    }
}