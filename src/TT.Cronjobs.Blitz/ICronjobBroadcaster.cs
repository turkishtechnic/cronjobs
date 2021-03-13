using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TT.Cronjobs.AspNetCore;

namespace TT.Cronjobs.Blitz
{
    internal interface ICronjobBroadcaster
    {
        Task BroadcastAsync(IEnumerable<CronjobWebhook> jobs, CancellationToken cancellationToken);
    }
}