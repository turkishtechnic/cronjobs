using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace TT.Cronjobs.Tests
{
    public class CronjobWithoutCronAttribute : ICronjob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [Cron("* * * * *")]
    public class SimpleCronjob : ICronjob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
    
    [Cron("* * * * *")]
    internal class HiddenCronjob : ICronjob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [Cron("* * * * *")]
    [DisplayName("title")]
    [Description("description")]
    public class AnnotatedCronjob : ICronjob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class AssemblyCronjobProviderTests
    {
        [Fact]
        public void MissingCronAttributePreventsDiscovery()
        {
            var provider = MakeAssemblyCronjobProvider();
            
            var invalidCronjob = typeof(CronjobWithoutCronAttribute);
            var httpCronjob = provider.BuildHttpCronjob(invalidCronjob);
            
            Assert.Null(httpCronjob);
        }

        [Fact]
        public void CronjobIsDiscoverable()
        {
            var provider = MakeAssemblyCronjobProvider();
            Assert.NotEmpty(provider.CronJobs);
            Assert.Contains(provider.CronJobs, cronjob => cronjob.Type == typeof(SimpleCronjob));
            Assert.DoesNotContain(provider.CronJobs, cronjob => cronjob.Type == typeof(HiddenCronjob));
        }

        [Theory]
        [InlineData(typeof(AnnotatedCronjob), "title", "description")]
        [InlineData(typeof(SimpleCronjob), nameof(SimpleCronjob), null)]
        public void CronjobAttributesOverridesDefaultNames(Type type, string title, string description)
        {
            var provider = MakeAssemblyCronjobProvider();
            var httpCronjob = provider.BuildHttpCronjob(type);
            Assert.Equal(httpCronjob.Title, title);
            Assert.Equal(httpCronjob.Description, description ?? httpCronjob.Type.FullName);
        }

        private static AssemblyCronjobProvider MakeAssemblyCronjobProvider()
        {
            var options = Options.Create(new CronjobsOptions
            {
                UrlTemplate = "{name}",
                PublicUrl = "https://example.com"
            });
            var provider = new AssemblyCronjobProvider(
                options,
                new NullLogger<AssemblyCronjobProvider>(),
                Assembly.GetExecutingAssembly()
            );
            return provider;
        }
    }
}