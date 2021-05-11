using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TT.Cronjobs.AspNetCore;
using Xunit;

namespace TT.Cronjobs.Tests
{
    public class WebhookTests
    {
        [Theory]
        [InlineData("https://example.com", typeof(SimpleCronjob))]
        [InlineData("https://example.com/subapp", typeof(SimpleCronjob))]
        public void WebhookUrlsAreGeneratedCorrectly(string baseUrl, Type cronjobType)
        {
            var providers = new List<ICronjobProvider>
            {
                CreateAssemblyCronjobProvider(),
            };
            var webhookProvider = new CronjobWebhookProvider(providers, Options.Create<CronjobsOptions>(new CronjobsOptions
            {
                RoutePattern = "/webhooks",
                WebhookBaseUrl = baseUrl
            }));

            var webhooks = webhookProvider.Cronjobs.ToList();
            Assert.Contains(webhooks, webhook => webhook.Url == $"{baseUrl}/webhooks/{cronjobType.Name.ToLowerInvariant()}");
        }
        
        private static AssemblyCronjobProvider CreateAssemblyCronjobProvider()
        {
            var provider = new AssemblyCronjobProvider(
                new NullLogger<AssemblyCronjobProvider>(),
                Assembly.GetExecutingAssembly()
            );
            return provider;
        }
    }
}