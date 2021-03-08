using System.Threading;
using System.Threading.Tasks;

namespace AbdusCo.CronJobs
{
    public interface ICronjob
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}