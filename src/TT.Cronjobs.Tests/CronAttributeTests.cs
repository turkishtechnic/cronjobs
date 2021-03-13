using System;
using Xunit;

namespace TT.Cronjobs.Tests
{
    public class CronAttributeTests
    {
        [Fact]
        public void CronParsingWorks()
        {
            var attribute = new CronAttribute("* * * * *");
            Assert.NotNull(attribute.Cron);
            Assert.Throws<FormatException>(() =>
            {
                var _ = new CronAttribute("*");
            });
        }
    }
}