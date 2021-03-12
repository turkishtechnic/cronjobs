using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TT.Cronjobs.AspNetCore
{
    public class CronjobsBuilder
    {
        public IServiceCollection Services { get; set; }
    }
    public static class ServiceCollectionExtensions
    {
        
        public static CronjobsBuilder AddCronjobs(
            this IServiceCollection services,
            Action<CronjobsOptions> configure = null,
            params Assembly[] assemblies
        )
        {
            if (configure != null)
            {
                services.Configure(configure);
            }
            
            services.AddTransient<ICronjobBroadcaster, CronjobRegistrationBroadcaster>();

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

            return new CronjobsBuilder{Services = services};
        }
    }
}