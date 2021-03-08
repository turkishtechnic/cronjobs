using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace TT.Cronjobs.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCronjobs(
            this IServiceCollection services,
            Action<CronjobsOptions> configure = null,
            params Assembly[] assemblies
        )
        {
            if (configure != null)
            {
                services.Configure(configure);
            }

            services.AddHttpClient<ICronjobBroadcaster, CronjobRegistrationBroadcaster>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptions<CronjobsOptions>>().Value;
                    client.BaseAddress = new Uri(options.RegistrationApiUrl);
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                })
                .AddPolicyHandler((provider, _) =>
                {
                    var options = provider.GetRequiredService<IOptions<CronjobsOptions>>().Value;
                    var builder = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(options.RetryCount, i => TimeSpan.FromSeconds(Math.Pow(2, i)));
                    return builder;
                });

            services.AddHostedService<JobBroadcasterService>();
            services.AddHostedService<CronJobExecutorBackgroundService>();
            services.AddSingleton<ICronjobFactory, CronjobFactory>();
            services.AddSingleton<ICronjobQueue, SynchronizedCronjobQueue>();

            if (assemblies == null || !assemblies.Any())
            {
                assemblies = new[] {Assembly.GetCallingAssembly()};
            }

            services.AddTransient<ICronjobProvider, AssemblyCronjobProvider>(
                provider =>
                {
                    var options = provider.GetRequiredService<IOptions<CronjobsOptions>>();
                    return new AssemblyCronjobProvider(options, assemblies);
                });

            return services;
        }
    }
}