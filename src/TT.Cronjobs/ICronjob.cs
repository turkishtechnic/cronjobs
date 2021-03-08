using System.Threading;
using System.Threading.Tasks;

namespace TT.Cronjobs
{
    public interface ICronjob
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}