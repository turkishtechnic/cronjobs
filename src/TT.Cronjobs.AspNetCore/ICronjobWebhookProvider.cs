using System.Collections.Generic;

namespace TT.Cronjobs.AspNetCore
{
    public interface ICronjobWebhookProvider
    {
        IEnumerable<CronjobWebhook> Cronjobs { get; }
    }
}