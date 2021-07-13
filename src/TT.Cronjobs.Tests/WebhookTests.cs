using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
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

        [Fact]
        public async Task AnonymousUserCannotAccessResources()
        {
            var authService = BuildAuthorizationService(services => services.AddCronjobs());

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var result = await authService.AuthorizeAsync(anonymousUser, "cronjob");

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task AuthenticationWorks()
        {
            var authService = BuildAuthorizationService(services => services.AddCronjobs());

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("cronjob", "true") }));
            var result = await authService.AuthorizeAsync(user, "cronjob");

            Assert.True(result.Succeeded);
        }

        private IAuthorizationService BuildAuthorizationService(Action<IServiceCollection> setupServices = null)
        {
            var services = new ServiceCollection();
            services.AddAuthorizationCore();
            services.AddLogging();
            services.AddOptions();
            setupServices?.Invoke(services);
            return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
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