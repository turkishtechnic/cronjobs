using System;
using System.Reflection;

namespace TT.Cronjobs
{
    public class CronjobInfo
    {
        public CronjobInfo(Type type)
        {
            Type = type;
            var cronAttr = type.GetCustomAttribute<CronAttribute>();
            Cron = cronAttr?.Cron ?? throw new ArgumentNullException(nameof(type), $"{type.Name} is not annotated with {nameof(CronAttribute)}");
            Name = type.Name;
        }

        public Type Type { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Cron { get; set; }
    }
}