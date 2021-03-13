using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

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
}