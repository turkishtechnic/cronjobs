using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TT.Cronjobs.AspNetCore
{
    internal class CronjobRegistrationBroadcaster : ICronjobBroadcaster
    {
        private readonly ICronjobApiClient _cronjobApi;
        private readonly IHostEnvironment _environment;

        public CronjobRegistrationBroadcaster(IHostEnvironment environment,
                                              ICronjobApiClient cronjobApi)
        {
            _environment = environment;
            _cronjobApi = cronjobApi;
        }

        public async Task BroadcastAsync(List<HttpCronjob> jobs, CancellationToken cancellationToken)
        {
            var buildId = Assembly.GetEntryAssembly()!.ManifestModule.ModuleVersionId.ToString();
            var payload = new ProjectBatchRegistration
            {
                Title = $"{_environment.ApplicationName} ({_environment.EnvironmentName})",
                Version = buildId,
                Cronjobs = jobs
            };
            await _cronjobApi.BatchRegisterProjectAsync(payload, cancellationToken);
        }
    }
}