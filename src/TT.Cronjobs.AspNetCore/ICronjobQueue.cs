using System.Threading;
using System.Threading.Tasks;

namespace TT.Cronjobs.AspNetCore
{
    public interface ICronjobQueue
    {
        Task EnqueueAsync(CronjobExecutionContext job);
        Task<CronjobExecutionContext> DequeueAsync(CancellationToken cancellationToken);
    }
}