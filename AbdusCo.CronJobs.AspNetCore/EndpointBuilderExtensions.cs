using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AbdusCo.CronJobs.AspNetCore
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapCronJobWebhook(this IEndpointRouteBuilder endpoints,
            string endpoint = "/-/cronjobs")
        {
            endpoints.MapGet(endpoint, context =>
            {
                var providers = context.RequestServices.GetRequiredService<IEnumerable<ICronJobProvider>>();
                var jobs = providers.SelectMany(p => p.CronJobs).ToList();

                var payload = JsonSerializer.Serialize(jobs,
                    new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                context.Response.ContentType = MediaTypeNames.Application.Json;
                return context.Response.WriteAsync(payload);
            });

            return endpoints.MapPost($"{endpoint}/{{name:required}}", async context =>
            {
                if (!(context.GetRouteValue("name") is string jobName))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                var factory = context.RequestServices.GetRequiredService<ICronJobFactory>();
                var executor = context.RequestServices.GetRequiredService<ICronJobQueue>();

                var job = factory.Create(jobName);
                await executor.EnqueueAsync(job).ConfigureAwait(false);

                context.Response.StatusCode = StatusCodes.Status202Accepted;
            });
        }
    }
}