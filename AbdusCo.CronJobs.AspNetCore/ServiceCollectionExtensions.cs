using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace AbdusCo.CronJobs.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCronJobs(
            this IServiceCollection services,
            Action<CronJobsOptions> configure = null,
            params Assembly[] assemblies
        )
        {
            if (configure != null)
            {
                services.Configure(configure);
            }


            services.AddHttpClient<ICronJobBroadcaster, CronJobRegistrationBroadcaster>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptions<CronJobsOptions>>().Value;
                    client.BaseAddress = new Uri(options.RegistrationEndpoint);
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                })
                .AddPolicyHandler((provider, _) =>
                {
                    var options = provider.GetRequiredService<IOptions<CronJobsOptions>>().Value;
                    var builder = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(options.RetryCount, i => TimeSpan.FromSeconds(Math.Pow(2, i)));
                    return builder;
                });

            services.AddHostedService<JobBroadcasterService>();
            services.AddHostedService<CronJobExecutorBackgroundService>();
            services.AddSingleton<ICronJobFactory, CronJobFactory>();
            services.AddSingleton<ICronJobQueue, SynchronizedCronJobQueue>();

            if (!assemblies.Any())
            {
                assemblies = new[] {Assembly.GetCallingAssembly()};
            }

            services.AddTransient<ICronJobProvider, AssemblyScanningCronJobProvider>(
                provider =>
                {
                    var options = provider.GetRequiredService<IOptions<CronJobsOptions>>();
                    return new AssemblyScanningCronJobProvider(options, assemblies);
                });

            return services;
        }

        public static IServiceCollection AddCronJobs(this IServiceCollection services)
            => AddCronJobs(services, assemblies: Assembly.GetEntryAssembly());
    }
}