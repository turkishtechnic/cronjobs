using System;
using System.Threading;
using System.Threading.Tasks;
using TT.Cronjobs;

namespace SampleWebProject.Jobs
{
    [Cron("*/5 * * * *")]
    public class FailingCronjob : ICronjob
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            throw new Exception("Something bad happened");
        }
    }
}