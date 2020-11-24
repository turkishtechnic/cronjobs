using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AbdusCo.CronJobs.AspNetCore
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapCronJobWebhook(this IEndpointRouteBuilder endpoints,
            string endpoint = "/-/jobs")
        {
            endpoints.MapGet(endpoint, context =>
            {
                var providers = context.RequestServices.GetRequiredService<IEnumerable<ICronJobProvider>>();
                var jobs = providers.SelectMany(p => p.CronJobs).ToList();
                
                var payload = JsonSerializer.Serialize(jobs);
                context.Response.ContentType = MediaTypeNames.Application.Json;
                return context.Response.WriteAsync(payload);
            });

            return endpoints.MapPost($"{endpoint}/{{name:required}}", context =>
            {
                if (!(context.GetRouteValue("name") is string jobName))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                }

                var factory = context.RequestServices.GetRequiredService<ICronJobFactory>();
                var executor = context.RequestServices.GetRequiredService<ICronJobExecutor>();

                context.Response.OnCompleted(async () =>
                {
                    var job = factory.Create(jobName);
                    await executor.ExecuteJobAsync(job).ConfigureAwait(false);
                });

                context.Response.StatusCode = StatusCodes.Status202Accepted;
                return Task.CompletedTask;
            });
        }
    }
}