using System;
using System.Linq;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace AbdusCo.CronJobs.Hangfire
{
    public class CronJobRegistry : ICronJobRegistry
    {
        private readonly IRecurringJobManager _jobManager;
        private readonly JobStorage _jobStorage;
        private readonly ILogger<CronJobRegistry> _logger;

        public CronJobRegistry(IRecurringJobManager jobManager, JobStorage jobStorage, ILogger<CronJobRegistry> logger)
        {
            _jobManager = jobManager;
            _jobStorage = jobStorage;
            _logger = logger;
        }

        public void Register(CronJobBroadcast payload)
        {
            var appId = $"{payload.Application}@{payload.Environment}";
            var buildId = payload.Build.ToString().Substring(0, 8);
            var appBuildId = $"{appId}@{buildId}";
            
            string MakeKey(CronJobDescription j, int i) => $"{appBuildId}.{j.Name}#{i}";

            _logger.LogDebug("Looking for registered jobs for {Application}", appId);
            
            var existingJobs = _jobStorage.GetConnection().GetRecurringJobs()
                .Where(j => j.Id.StartsWith(appId))
                .ToList();
            var jobsFromSameBuild = existingJobs.Where(j => j.Id.StartsWith(appBuildId)).ToList();
            var jobsFromOtherBuilds = existingJobs.Except(jobsFromSameBuild);

            _logger.LogDebug($"Found {existingJobs.Count} jobs. Replacing with new ones.");
            
            foreach (var job in jobsFromOtherBuilds)
            {
                _jobManager.RemoveIfExists(job.Id);
            }

            var jobs = payload.Jobs.ToList();
            var total = 0;
            foreach (var job in jobs)
            {
                _logger.LogDebug("Registering {Job} for {Application}", job, payload.Application);
                for (var i = 0; i < job.CronExpressions.Count; i++)
                {
                    var cron = job.CronExpressions[i];
                    var jobId = MakeKey(job, i);

                    if (jobsFromSameBuild.Any(j => j.Id == jobId)) continue;
                    
                    _jobManager.AddOrUpdate<CronJobTriggerer>(
                        jobId,
                        t => t.Trigger(job),
                        cron,
                        TimeZoneInfo.Local
                    );
                    total++;
                }
            }

            _logger.LogInformation($"Registered {total} jobs for {{Application}}.", appId);
        }
    }
}