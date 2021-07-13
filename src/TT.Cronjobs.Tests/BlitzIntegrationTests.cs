using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TT.Cronjobs.AspNetCore;
using TT.Cronjobs.Blitz;
using Xunit;

namespace TT.Cronjobs.Tests
{
    public partial class BlitzTests
    {
        [Fact]
        public async Task CronjobsAreRegisteredAtStartup()
        {
            var blitzClient = new FakeBlitzClient();
            var versionProvider = new FakeVersionProvider("testversion");
            var host = await CreateHost(options =>
                {
                    options.WaitSeconds = 0;
                    options.RetryCount = 0;
                    options.ApiBaseUrl = "https://example.com";
                },
                apiClient: blitzClient,
                versionProvider: versionProvider
            );

            // just in case
            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotNull(blitzClient.ProjectBatchRegistration);
            Assert.NotEmpty(blitzClient.ProjectBatchRegistration.Cronjobs);
            Assert.Equal(versionProvider.Version, blitzClient.ProjectBatchRegistration.Version);
        }

        [Fact]
        public async Task CronjobStatusUpdatesAreSent()
        {
            var mockClient = new Mock<ICronjobApiClient>();
            mockClient
                .Setup(it => it.UpdateExecutionStatusAsync(It.IsAny<string>(), It.IsAny<StatusUpdate>(), default))
                .Returns(Task.CompletedTask);

            var monitor = new BlitzExecutionMonitor(mockClient.Object);
            var executor = new CronjobExecutor(monitor, new NullLogger<CronjobExecutor>());
            await executor.ExecuteAsync(new CronjobExecutionContext(new SimpleCronjob(), "1"));

            mockClient.Verify(
                m => m.UpdateExecutionStatusAsync("1", It.IsAny<StatusUpdate>(), default),
                Times.Exactly(2)
            );

            await executor.ExecuteAsync(new CronjobExecutionContext(new FailingCronjob(), "1"));
            mockClient.Verify(
                m => m.UpdateExecutionStatusAsync("1", It.Is<StatusUpdate>(update => update.State == ExecutionState.Failed.Name), default),
                Times.Once
            );
        }

        [Fact]
        public void MissingConfigurationThrowsError()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(conf => conf.GetSection(It.Is<string>(s => s != CronjobsOptions.Key)))
                .Returns(It.IsAny<IConfigurationSection>());
            var configurator = new CronjobsBuilderExtensions.ConfigureBlitz(mockConfig.Object);

            Assert.Throws<ApplicationException>(() => configurator.Configure(new BlitzOptions()));
        }

        private async Task<IHost> CreateHost(Action<BlitzOptions> configureBlitz,
                                             ICronjobApiClient apiClient,
                                             IVersionProvider versionProvider = null,
                                             Assembly assembly = null)
        {
            var host = new HostBuilder()
                .ConfigureWebHost(builder =>
                {
                    builder.UseTestServer()
                        .ConfigureServices(services
                            =>
                        {
                            services
                                .AddCronjobs(options =>
                                    {
                                        options.RoutePattern = "cronjobs";
                                        options.WebhookBaseUrl = "https://example.com";
                                    },
                                    assembly ?? Assembly.GetExecutingAssembly()
                                )
                                .UseBlitz(configureBlitz);
                            services.AddSingleton<ICronjobApiClient>(apiClient);
                            services.AddTransient<ICronjobProvider, FakeCronjobProvider>();
                            services.AddTransient<IVersionProvider>(provider => versionProvider ?? new StubVersionProvider());
                            services.AddTransient<SimpleCronjob>();

                            services.AddRouting();
                        })
                        .Configure(app
                            => app.UseRouting().UseEndpoints(routeBuilder => { routeBuilder.MapCronjobWebhook(); }));
                }).Build();
            await host.StartAsync();
            return host;
        }

        class FakeBlitzClient : ICronjobApiClient
        {
            public ProjectBatchRegistration ProjectBatchRegistration { get; set; }
            public Dictionary<string, StatusUpdate> StatusUpdates { get; set; } = new Dictionary<string, StatusUpdate>();

            public Task BatchRegisterProjectAsync(ProjectBatchRegistration registration, CancellationToken cancellationToken = default)
            {
                ProjectBatchRegistration = registration;
                return Task.CompletedTask;
            }

            public Task UpdateExecutionStatusAsync(string executionId, StatusUpdate update, CancellationToken cancellationToken = default)
            {
                StatusUpdates.Add(executionId, update);
                return Task.CompletedTask;
            }
        }

        class FakeVersionProvider : IVersionProvider
        {
            private readonly string _version;
            public FakeVersionProvider(string version) => _version = version;
            public string Version => _version;
        }

        class StubVersionProvider : IVersionProvider
        {
            public string Version => "version";
        }
    }
}