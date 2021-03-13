using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TT.Cronjobs.AspNetCore;

namespace TT.Cronjobs.Blitz
{
    internal interface ICronjobBroadcaster
    {
        Task BroadcastAsync(CancellationToken cancellationToken = default);
    }

    internal class CronjobRegistrationBroadcaster : ICronjobBroadcaster
    {
        private readonly ICronjobApiClient _cronjobApi;
        private readonly IHostEnvironment _environment;
        private readonly IVersionProvider _versionProvider;
        private readonly ILogger<CronjobRegistrationBroadcaster> _logger;
        private readonly CronjobWebhookProvider _cronjobWebhookProvider;

        public CronjobRegistrationBroadcaster(IHostEnvironment environment,
                                              ICronjobApiClient cronjobApi,
                                              IVersionProvider versionProvider,
                                              ILogger<CronjobRegistrationBroadcaster> logger,
                                              CronjobWebhookProvider cronjobWebhookProvider)
        {
            _environment = environment;
            _cronjobApi = cronjobApi;
            _versionProvider = versionProvider;
            _logger = logger;
            _cronjobWebhookProvider = cronjobWebhookProvider;
        }

        public async Task BroadcastAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Finding cronjobs");
            List<CronjobWebhook> jobs;
            try
            {
                jobs = _cronjobWebhookProvider.Cronjobs.ToList();
                _logger.LogInformation($"Found {jobs.Count} cronjobs");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while discovering cronjobs");
                throw;
            }

            var payload = new ProjectBatchRegistration
            {
                Title = $"{_environment.ApplicationName} ({_environment.EnvironmentName})",
                Version = _versionProvider.Version,
                Cronjobs = jobs
            };
            await _cronjobApi.BatchRegisterProjectAsync(payload, cancellationToken);
        }
    }
}