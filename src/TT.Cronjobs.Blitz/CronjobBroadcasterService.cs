using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TT.Cronjobs.Blitz
{
    internal class CronjobBroadcasterService : BackgroundService
    {
        private readonly BlitzOptions _options;
        private readonly ICronjobBroadcaster _broadcaster;
        private readonly ILogger<CronjobBroadcasterService> _logger;

        public CronjobBroadcasterService(
            IOptions<BlitzOptions> options,
            ICronjobBroadcaster broadcaster,
            ILogger<CronjobBroadcasterService> logger)
        {
            _options = options.Value;
            _broadcaster = broadcaster;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(_options.WaitSeconds), stoppingToken);
            
            try
            {
                await _broadcaster.BroadcastAsync(stoppingToken);
                _logger.LogInformation("Jobs have been broadcast successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to broadcast cronjobs.");
            }
        }
    }
}