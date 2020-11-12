﻿using System;
using System.Threading.Tasks;
using AbdusCo.CronJobs.AspNetCore;
using Microsoft.Extensions.Logging;

namespace HangfireDemo.Jobs
{
    [Cron("*/1 * * * *", "1 1 * * *")]
    public class CreateReport : IJob
    {
        private readonly ILogger<CreateReport> _logger;

        public CreateReport(ILogger<CreateReport> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("creating report...");
            await Task.Delay(TimeSpan.FromSeconds(10));
            _logger.LogInformation("created report");
        }
    }
}