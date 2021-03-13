using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TT.Cronjobs
{
    public class AssemblyCronjobProvider : ICronjobProvider
    {
        private readonly CronjobsOptions _options;
        private readonly Assembly[] _assemblies;
        private readonly ILogger<AssemblyCronjobProvider> _logger;

        public AssemblyCronjobProvider(IOptions<CronjobsOptions> options,
                                       ILogger<AssemblyCronjobProvider> logger,
                                       params Assembly[] assemblies)
        {
            _options = options.Value;
            _logger = logger;
            _assemblies = assemblies;
        }

        public IEnumerable<HttpCronjob> CronJobs => GetCronjobs();

        private IEnumerable<HttpCronjob> GetCronjobs()
        {
            return _assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && t.IsPublic
                            && typeof(ICronjob).IsAssignableFrom(t))
                .Select(BuildHttpCronjob)
                .Where(it => it != null);
        }

        public HttpCronjob BuildHttpCronjob(Type type)
        {
            var cronAttr = type.GetCustomAttribute<CronAttribute>();
            if (cronAttr == null)
            {
                _logger.LogWarning($"{{Cronjob}} is not annotated with {nameof(CronAttribute)}", type.FullName);
                return null;
            }

            var title = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? type.Name;
            var description = type.GetCustomAttribute<DescriptionAttribute>()?.Description ?? type.FullName;

            return new HttpCronjob
            {
                Type = type,
                Title = title,
                Description = description,
                Url = _options.MakeUrl(type.Name.ToLowerInvariant()),
                Cron = cronAttr.Cron,
                HttpMethod = "POST"
            };
        }
    }
}