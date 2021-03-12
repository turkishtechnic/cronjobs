using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using TT.Cronjobs.AspNetCore;

namespace TT.Cronjobs.Blitz
{
    public static class CronjobsBuilderExtensions
    {
        public static CronjobsBuilder UseBlitz(this CronjobsBuilder builder)
        {
            builder.Services.AddHttpClient<ICronjobApiClient, BlitzCronjobApiClient>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptions<CronjobsOptions>>().Value;
                    client.BaseAddress = new Uri(options.ApiBaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                })
                .AddPolicyHandler((provider, _) =>
                {
                    var options = provider.GetRequiredService<IOptions<CronjobsOptions>>().Value;
                    var policy = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(options.RetryCount, i => TimeSpan.FromSeconds(Math.Pow(2, i)));
                    return policy;
                });

            return builder;
        }
    }
}