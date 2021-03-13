using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace TT.Cronjobs.Tests
{
    public class AssemblyCronjobProviderTests
    {
        [Fact]
        public void MissingCronAttributePreventsDiscovery()
        {
            var provider = MakeAssemblyCronjobProvider();

            var invalidCronjob = typeof(CronjobWithoutCronAttribute);
            var info = provider.BuildCronjobInfo(invalidCronjob);

            Assert.Null(info);
        }

        [Fact]
        public void CronjobIsDiscoverable()
        {
            var provider = MakeAssemblyCronjobProvider();
            Assert.NotEmpty(provider.Cronjobs);
            Assert.Contains(provider.Cronjobs, cronjob => cronjob.Type == typeof(SimpleCronjob));
            Assert.DoesNotContain(provider.Cronjobs, cronjob => cronjob.Type == typeof(HiddenCronjob));
        }

        [Theory]
        [InlineData(typeof(AnnotatedCronjob), "title", "description")]
        [InlineData(typeof(SimpleCronjob), nameof(SimpleCronjob), null)]
        public void CronjobAttributesOverridesDefaultNames(Type type, string title, string description)
        {
            var provider = MakeAssemblyCronjobProvider();
            var info = provider.BuildCronjobInfo(type);
            Assert.Equal(info.Title, title);
            Assert.Equal(info.Description, description ?? info.Type.FullName);
        }

        private static AssemblyCronjobProvider MakeAssemblyCronjobProvider()
        {
            var provider = new AssemblyCronjobProvider(
                new NullLogger<AssemblyCronjobProvider>(),
                Assembly.GetExecutingAssembly()
            );
            return provider;
        }
    }

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
}