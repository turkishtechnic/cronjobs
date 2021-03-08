using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AbdusCo.CronJobs
{
    public class CronjobFactory : ICronjobFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CronjobFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public ICronjob Create(string jobName)
        {
            var type = Assembly.GetEntryAssembly()!
                .GetTypes()
                .FirstOrDefault(t =>
                    t.IsClass
                    && typeof(ICronjob).IsAssignableFrom(t)
                    && string.Equals(t.Name, jobName, StringComparison.InvariantCultureIgnoreCase));

            if (type == null)
            {
                throw new TypeLoadException($"Cannot find any job named {jobName} that implements {nameof(ICronjob)}");
            }

            return (ICronjob) _scopeFactory.CreateScope().ServiceProvider.GetRequiredService(type);
        }
    }
}