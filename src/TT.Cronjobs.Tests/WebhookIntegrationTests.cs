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
using Moq;
using TT.Cronjobs.AspNetCore;
using Xunit;

namespace TT.Cronjobs.Tests
{
    public class WebhookIntegrationTests
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
            var mockMonitor = new Mock<ICronjobExecutionMonitor>();
            mockMonitor.Setup(monitor => monitor.StartedAsync(It.IsAny<CronjobExecutionContext>())).Returns(Task.CompletedTask);
            mockMonitor.Setup(monitor => monitor.FinishedAsync(It.IsAny<CronjobExecutionContext>())).Returns(Task.CompletedTask);

            var host = await CreateHost(options =>
            {
                options.RoutePattern = "cronjobs";
                options.WebhookBaseUrl = "https://example.com";
            }, services => services.AddTransient<ICronjobExecutionMonitor>(_ => mockMonitor.Object));

            var client = host.GetTestClient();
            var req = new HttpRequestMessage(HttpMethod.Post, $"cronjobs/{nameof(SimpleCronjob)}");
            req.Headers.Add("Execution-Id", "1");
            await client.SendAsync(req);

            mockMonitor.Verify(m => m.StartedAsync(It.IsAny<CronjobExecutionContext>()), Times.Once);
            mockMonitor.Verify(m => m.FinishedAsync(It.IsAny<CronjobExecutionContext>()), Times.Once);
        }


        [Fact]
        public async Task ExecutionFailedEventIsDispatched()
        {
            var mockMonitor = new Mock<ICronjobExecutionMonitor>();
            mockMonitor.Setup(monitor => monitor.FailedAsync(It.IsAny<CronjobExecutionContext>(), It.IsAny<Exception>())).Returns(Task.CompletedTask);

            var host = await CreateHost(options =>
            {
                options.RoutePattern = "cronjobs";
                options.WebhookBaseUrl = "https://example.com";
            }, services => services.AddTransient<ICronjobExecutionMonitor>(_ => mockMonitor.Object));

            var client = host.GetTestClient();
            var req = new HttpRequestMessage(HttpMethod.Post, $"cronjobs/{nameof(FailingCronjob)}");
            req.Headers.Add("Execution-Id", "1");
            await client.SendAsync(req);

            // just in case
            await Task.Delay(TimeSpan.FromSeconds(1));

            mockMonitor.Verify(m => m.FailedAsync(It.IsAny<CronjobExecutionContext>(), It.IsAny<Exception>()), Times.Once);
        }


        class FakeCronjobProvider : ICronjobProvider
        {
            public IEnumerable<CronjobInfo> Cronjobs { get; } = new[]
            {
                new CronjobInfo(typeof(SimpleCronjob)),
                new CronjobInfo(typeof(FailingCronjob)),
            };
        }

        private async Task<IHost> CreateHost(Action<CronjobsOptions> configure, Action<IServiceCollection> configureServices = null)
        {
            var host = new HostBuilder()
                .ConfigureWebHost(builder =>
                {
                    builder.UseTestServer()
                        .ConfigureServices(services
                            =>
                        {
                            services.AddTransient<SimpleCronjob>();
                            services.AddTransient<FailingCronjob>();
                            services.AddRouting();
                            services.AddCronjobs(configure, Assembly.GetExecutingAssembly());
                            services.AddTransient<ICronjobProvider, FakeCronjobProvider>();
                            configureServices?.Invoke(services);
                        })
                        .Configure(app
                            => app.UseRouting().UseEndpoints(routeBuilder => { routeBuilder.MapCronjobWebhook(); }));
                }).Build();
            await host.StartAsync();
            return host;
        }
    }
}