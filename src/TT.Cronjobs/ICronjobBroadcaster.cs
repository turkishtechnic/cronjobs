using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TT.Cronjobs
{
    public interface ICronjobBroadcaster
    {
        Task BroadcastAsync(List<CronjobInfo> jobs, CancellationToken cancellationToken);
    }
}