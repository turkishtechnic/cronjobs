using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using TT.Cronjobs.AspNetCore;

[assembly: InternalsVisibleTo("TT.Cronjobs.Tests")]

namespace TT.Cronjobs.Blitz
{
    public static class CronjobsBuilderExtensions
    {
        public static CronjobsBuilder UseBlitz(this CronjobsBuilder builder,
                                               Action<BlitzOptions> configure = null)
        {
            if (configure != null)
            {
                builder.Services.Configure(configure);
            }
            else
            {
                builder.Services.AddSingleton<IConfigureOptions<BlitzOptions>, ConfigureBlitz>();
            }

            builder.Services.AddTransient<IVersionProvider>(provider => new AssemblyVersionProvider(builder.Assembly));
            builder.Services.AddTransient<ICronjobBroadcaster, CronjobRegistrationBroadcaster>();
            builder.Services.AddTransient<ICronjobExecutionMonitor, BlitzExecutionMonitor>();
            builder.Services.AddHostedService<CronjobBroadcasterService>();

            builder.Services.AddHttpClient<ICronjobApiClient, BlitzCronjobApiClient>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptions<BlitzOptions>>().Value;
                    client.BaseAddress = new Uri(options.ApiBaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                })
                .AddPolicyHandler((provider, _) =>
                {
                    var options = provider.GetRequiredService<IOptions<BlitzOptions>>().Value;
                    var policy = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(options.RetryCount, i => TimeSpan.FromSeconds(Math.Pow(2, i)));
                    return policy;
                });

            return builder;
        }

        internal class ConfigureBlitz : IConfigureOptions<BlitzOptions>
        {
            private readonly IConfiguration _configuration;
            public ConfigureBlitz(IConfiguration configuration) => _configuration = configuration;

            public void Configure(BlitzOptions options)
            {
                var section = _configuration.GetSection(CronjobsOptions.Key) ??
                              throw new ApplicationException($"Missing configuration keyed '{CronjobsOptions.Key}'");
                section.Bind(options);
            }
        }
    }
}