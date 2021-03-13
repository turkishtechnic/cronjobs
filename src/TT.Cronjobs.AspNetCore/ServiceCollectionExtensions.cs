using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TT.Cronjobs.AspNetCore
{
    public class CronjobsBuilder
    {
        public CronjobsBuilder(IServiceCollection services) => Services = services;
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
            else
            {
                services.AddSingleton<IConfigureOptions<CronjobsOptions>, ConfigureCronjobs>();
            }

            if (assemblies == null || !assemblies.Any())
            {
                assemblies = new[] {Assembly.GetCallingAssembly()};
            }

            services.AddHostedService<CronjobExecutorBackgroundService>();
            services.AddSingleton<ICronjobFactory, ScopedCronjobFactory>();
            services.AddSingleton<ICronjobQueue, SynchronizedCronjobQueue>();
            services.AddTransient<CronjobWebhookProvider>();

            services.AddTransient<ICronjobProvider, AssemblyCronjobProvider>(
                provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<AssemblyCronjobProvider>>();
                    return new AssemblyCronjobProvider(logger, assemblies);
                });

            return new CronjobsBuilder(services);
        }

        internal class ConfigureCronjobs : IConfigureOptions<CronjobsOptions>
        {
            private readonly IConfiguration _configuration;
            public ConfigureCronjobs(IConfiguration configuration) => _configuration = configuration;
            public void Configure(CronjobsOptions options) => _configuration.GetSection(CronjobsOptions.Key).Bind(options);
        }
    }
}