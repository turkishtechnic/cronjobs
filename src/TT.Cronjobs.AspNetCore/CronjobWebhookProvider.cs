using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace TT.Cronjobs.AspNetCore
{
    internal class CronjobWebhookProvider : ICronjobWebhookProvider
    {
        private readonly IEnumerable<ICronjobProvider> _cronjobProviders;
        private readonly CronjobsOptions _options;

        public CronjobWebhookProvider(IEnumerable<ICronjobProvider> cronjobProviders, IOptions<CronjobsOptions> options)
        {
            _cronjobProviders = cronjobProviders;
            _options = options.Value;
        }

        public IEnumerable<CronjobWebhook> Cronjobs => _cronjobProviders.SelectMany(it => it.Cronjobs)
            .Select(it =>
            {
                var path = $"{_options.RoutePattern.TrimEnd('/')}/{it.Name.ToLowerInvariant()}";
                var url = _options.WebhookBaseUrl == null
                    ? path
                    : new Uri(new Uri(_options.WebhookBaseUrl).WithTrailingSlash(), path.TrimStart('/')).ToString();
                return new CronjobWebhook
                {
                    Url = url,
                    Title = it.Title,
                    Description = it.Description,
                    Cron = it.Cron,
                    HttpMethod = HttpMethod.Post.Method,
                };
            });
    }
}