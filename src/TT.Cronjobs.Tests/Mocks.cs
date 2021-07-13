using System.Collections.Generic;

namespace TT.Cronjobs.Tests
{
    public partial class BlitzTests
    {
        private class FakeCronjobProvider : ICronjobProvider
        {
            public IEnumerable<CronjobInfo> Cronjobs { get; } = new[]
            {
                new CronjobInfo(typeof(SimpleCronjob))
            };
        }
    }
}