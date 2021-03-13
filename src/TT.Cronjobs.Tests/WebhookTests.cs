using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TT.Cronjobs.AspNetCore;
using Xunit;

namespace TT.Cronjobs.Tests
{
    public class WebhookTests
    {
        [Fact]
        public async Task CronjobsAreDiscovered()
        {
            var host = await CreateHost(options =>
            {
                options.RoutePattern = "cronjobs";
                options.WebhookBaseUrl = "https://example.com";
            });

            var client = host.GetTestClient();
            var response = await client.GetAsync("cronjobs");
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);
            var json = await response.Content.ReadAsStringAsync();

            var items = JsonSerializer.Deserialize<List<CronjobWebhook>>(json, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            Assert.NotEmpty(items);
            Assert.Equal(new CronjobInfo(typeof(SimpleCronjob)).Cron, items.First().Cron);
            Assert.Equal($"https://example.com/cronjobs/{nameof(SimpleCronjob).ToLowerInvariant()}", items.First().Url);
        }

        [Fact]
        public async Task CannotTriggerCronjobWithoutExecutionIdHeader()
        {
            var host = await CreateHost(options =>
            {
                options.RoutePattern = "cronjobs";
                options.WebhookBaseUrl = "https://example.com";
            });

            var client = host.GetTestClient();
            var response = await client.PostAsync($"cronjobs/{nameof(SimpleCronjob)}", new StringContent(""));
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CronjobIsTriggered()
        {
            var host = await CreateHost(options =>
            {
                options.RoutePattern = "cronjobs";
                options.WebhookBaseUrl = "https://example.com";
            });

            var client = host.GetTestClient();
            var req = new HttpRequestMessage(HttpMethod.Post, $"cronjobs/{nameof(SimpleCronjob)}");
            req.Headers.Add("Execution-Id", "1");
            var response = await client.SendAsync(req);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ExecutionEventsAreDispatched()
        {
            var host = await CreateHost(options =>
            {
                options.RoutePattern = "cronjobs";
                options.WebhookBaseUrl = "https://example.com";
                options.Events.OnStarted = context =>
                {
                    Assert.Equal("1", context.ExecutionId);
                    Assert.IsType<SimpleCronjob>(context.Cronjob);
                    return Task.CompletedTask;
                };
                options.Events.OnFinished = context =>
                {
                    Assert.Equal("1", context.ExecutionId);
                    Assert.IsType<SimpleCronjob>(context.Cronjob);
                    return Task.CompletedTask;
                };
            });

            var client = host.GetTestClient();
            var req = new HttpRequestMessage(HttpMethod.Post, $"cronjobs/{nameof(SimpleCronjob)}");
            req.Headers.Add("Execution-Id", "1");
            await client.SendAsync(req);
        }

        class FakeCronjobProvider : ICronjobProvider
        {
            public IEnumerable<CronjobInfo> Cronjobs { get; } = new[]
            {
                new CronjobInfo(typeof(SimpleCronjob))
            };
        }

        private async Task<IHost> CreateHost(Action<CronjobsOptions> configure)
        {
            var host = new HostBuilder()
                .ConfigureWebHost(builder =>
                {
                    builder.UseTestServer()
                        .ConfigureServices(services
                            =>
                        {
                            services.AddTransient<SimpleCronjob>();
                            services.AddRouting();
                            services.AddCronjobs(configure, Assembly.GetExecutingAssembly());
                            services.AddTransient<ICronjobProvider, FakeCronjobProvider>();
                        })
                        .Configure(app
                            => app.UseRouting().UseEndpoints(routeBuilder => { routeBuilder.MapCronjobWebhook(); }));
                }).Build();
            await host.StartAsync();
            return host;
        }
    }
}