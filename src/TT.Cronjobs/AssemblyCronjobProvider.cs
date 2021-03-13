using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace TT.Cronjobs
{
    public class AssemblyCronjobProvider : ICronjobProvider
    {
        private readonly Assembly[] _assemblies;
        private readonly ILogger<AssemblyCronjobProvider> _logger;

        public AssemblyCronjobProvider(ILogger<AssemblyCronjobProvider> logger,
                                       params Assembly[] assemblies)
        {
            _logger = logger;
            _assemblies = assemblies;
        }

        public IEnumerable<CronjobInfo> Cronjobs => GetCronjobs();

        private IEnumerable<CronjobInfo> GetCronjobs()
        {
            return _assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && t.IsPublic
                            && typeof(ICronjob).IsAssignableFrom(t))
                .Select(BuildCronjobInfo)
                .Where(it => it != null);
        }

        public CronjobInfo BuildCronjobInfo(Type type)
        {
            var cronAttr = type.GetCustomAttribute<CronAttribute>();
            if (cronAttr == null)
            {
                _logger.LogWarning($"{{Cronjob}} is not annotated with {nameof(CronAttribute)}", type.FullName);
                return null;
            }

            return new CronjobInfo(type)
            {
                Title = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? type.Name,
                Description = type.GetCustomAttribute<DescriptionAttribute>()?.Description ?? type.FullName,
            };
        }
    }
}