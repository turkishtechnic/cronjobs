using System;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace TT.Cronjobs.Tests
{
    public class AssemblyCronjobProviderTests
    {
        [Fact]
        public void MissingCronAttributePreventsDiscovery()
        {
            var provider = CreateAssemblyCronjobProvider();

            var invalidCronjob = typeof(CronjobWithoutCronAttribute);
            var info = provider.BuildCronjobInfo(invalidCronjob);

            Assert.Null(info);
        }

        [Fact]
        public void CronjobIsDiscoverable()
        {
            var provider = CreateAssemblyCronjobProvider();
            Assert.NotEmpty(provider.Cronjobs);
            Assert.Contains(provider.Cronjobs, cronjob => cronjob.Type == typeof(SimpleCronjob));
            Assert.DoesNotContain(provider.Cronjobs, cronjob => cronjob.Type == typeof(HiddenCronjob));
        }

        [Theory]
        [InlineData(typeof(AnnotatedCronjob), "title", "description")]
        [InlineData(typeof(SimpleCronjob), nameof(SimpleCronjob), null)]
        public void CronjobAttributesOverridesDefaultNames(Type type, string title, string description)
        {
            var provider = CreateAssemblyCronjobProvider();
            var info = provider.BuildCronjobInfo(type);
            Assert.Equal(info.Title, title);
            Assert.Equal(info.Description, description ?? info.Type.FullName);
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