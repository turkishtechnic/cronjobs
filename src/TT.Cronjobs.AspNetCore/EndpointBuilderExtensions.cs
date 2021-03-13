using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TT.Cronjobs.AspNetCore
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapCronjobWebhook(this IEndpointRouteBuilder endpoints,
                                                                   string endpoint = "/-/cronjobs")
        {
            var options = endpoints.ServiceProvider.GetRequiredService<IOptions<CronjobsOptions>>().Value;
            endpoints.MapGet(endpoint, context =>
            {
                var providers = context.RequestServices.GetRequiredService<IEnumerable<ICronjobProvider>>();
                var jobs = providers.SelectMany(p => p.CronJobs).ToList();

                var payload = JsonSerializer.Serialize(
                    jobs,
                    new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase}
                );
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

                var factory = context.RequestServices.GetRequiredService<ICronjobFactory>();
                var executorQueue = context.RequestServices.GetRequiredService<ICronjobQueue>();

                var executionId = context.Request.Headers["Execution-Id"].ToString();

                var job = factory.Create(jobName);
                await executorQueue.EnqueueAsync(new CronjobExecutionContext(Guid.Parse(executionId), job)).ConfigureAwait(false);

                context.Response.StatusCode = StatusCodes.Status202Accepted;
            });
        }
    }
}