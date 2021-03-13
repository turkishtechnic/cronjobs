using System.Collections.Generic;

namespace TT.Cronjobs
{
    public interface ICronjobProvider
    {
        IEnumerable<CronjobInfo> Cronjobs { get; }
    }
}