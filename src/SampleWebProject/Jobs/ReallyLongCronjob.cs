using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TT.Cronjobs;

namespace SampleWebProject.Jobs
{
    [Cron("*/5 * * * *")]
    [DisplayName("Heavy computation")]
    [Description("Performs a task that takes really long")]
    public class ReallyLongCronjob: ICronjob
    {
        private readonly ILogger<ReallyLongCronjob> _logger;

        public ReallyLongCronjob(ILogger<ReallyLongCronjob> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var watch = Stopwatch.StartNew();
            _logger.LogInformation("starting...");
            while (watch.Elapsed < TimeSpan.FromSeconds(20) && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                _logger.LogInformation($"still working (took {watch.Elapsed} so far)...");
            }
            _logger.LogInformation("done");
        }
    }
}