using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TT.Cronjobs;

namespace SampleWebProject.Jobs
{
    [Cron("5 * * * *")]
    [DisplayName("Hourly report")]
    public class CreateHourlyReport : ICronjob
    {
        private readonly ILogger<CreateHourlyReport> _logger;
        private readonly DemoDbContext _db;

        public CreateHourlyReport(ILogger<CreateHourlyReport> logger, DemoDbContext db)
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