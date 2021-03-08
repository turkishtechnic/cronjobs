using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TT.Cronjobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HangfireDemo.Jobs
{
    // TODO: use crontab exp builder
    [Cron("*/5 * * * *")]
    public class CreateReport : ICronjob
    {
        private readonly ILogger<CreateReport> _logger;
        private readonly DemoDbContext _db;

        public CreateReport(ILogger<CreateReport> logger, DemoDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating report...");
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            var sum = _db.Sales.Sum(s => s.Total);
            _logger.LogInformation($"created report. sum = {sum}");
        }
    }
}