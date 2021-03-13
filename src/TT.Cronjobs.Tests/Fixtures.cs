using System;
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
    public class FailingCronjob : ICronjob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => throw new Exception("something bad happened");
    }

    [Cron("* * * * *")]
    internal class NonPublicCronjob : ICronjob
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