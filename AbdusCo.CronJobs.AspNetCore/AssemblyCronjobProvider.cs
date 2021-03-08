using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace AbdusCo.CronJobs.AspNetCore
{
    public class AssemblyCronjobProvider : ICronjobProvider
    {
        private readonly string _urlTemplate;
        private readonly Assembly[] _assemblies;

        public AssemblyCronjobProvider(IOptions<CronjobsOptions> options,
                                       params Assembly[] assemblies)
        {
            _urlTemplate = options.Value.UrlTemplate;
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
                .SelectMany(CreateCronjobDescription);
        }

        private IEnumerable<HttpCronjob> CreateCronjobDescription(Type type)
        {
            var cronAttr = type.GetCustomAttribute<CronAttribute>() ??
                           throw new TypeLoadException($"{type} does not have any {nameof(CronAttribute)} attribute");

            var description = type.GetCustomAttribute<DescriptionAttribute>()?.Description
                              ?? type.FullName;

            for (var index = 0; index < cronAttr.CronExpressions.Length; index++)
            {
                var cron = cronAttr.CronExpressions[index];
                // dont append a suffix if there's only one cron schedule
                var suffix = index > 0 ? $".{index}" : "";
                yield return new HttpCronjob
                {
                    Title = $"{type.Name}{suffix}",
                    Description = description,
                    Url = _urlTemplate.Replace("{name}", type.Name.ToLowerInvariant()),
                    Cron = cron,
                    HttpMethod = "POST"
                };
            }
        }
    }
}