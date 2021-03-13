using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace TT.Cronjobs.AspNetCore
{
    public class ScopedCronjobFactory : ICronjobFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICronjobProvider _cronjobProvider;

        public ScopedCronjobFactory(IServiceScopeFactory scopeFactory, ICronjobProvider cronjobProvider)
        {
            _scopeFactory = scopeFactory;
            _cronjobProvider = cronjobProvider;
        }

        public ICronjob Create(string cronjobName)
        {
            var cronjob = _cronjobProvider.Cronjobs
                .SingleOrDefault(it => string.Equals(it.Name, cronjobName, StringComparison.InvariantCultureIgnoreCase));

            if (cronjob?.Type == null)
            {
                throw new ApplicationException($"No such cronjob named {cronjobName}");
            }

            return (ICronjob) _scopeFactory.CreateScope().ServiceProvider.GetRequiredService(cronjob.Type);
        }
    }
}