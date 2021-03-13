using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TT.Cronjobs.AspNetCore;

namespace TT.Cronjobs.Blitz
{
    internal class CronjobRegistrationBroadcaster : ICronjobBroadcaster
    {
        private readonly ICronjobApiClient _cronjobApi;
        private readonly IHostEnvironment _environment;
        private readonly IVersionProvider _versionProvider;

        public CronjobRegistrationBroadcaster(IHostEnvironment environment,
                                              ICronjobApiClient cronjobApi,
                                              IVersionProvider versionProvider)
        {
            _environment = environment;
            _cronjobApi = cronjobApi;
            _versionProvider = versionProvider;
        }

        public async Task BroadcastAsync(IEnumerable<CronjobWebhook> jobs, CancellationToken cancellationToken)
        {
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