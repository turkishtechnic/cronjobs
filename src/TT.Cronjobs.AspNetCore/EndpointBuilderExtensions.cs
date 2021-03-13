using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TT.Cronjobs.AspNetCore
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapCronjobWebhook(this IEndpointRouteBuilder endpoints)
        {
            var options = endpoints.ServiceProvider.GetRequiredService<IOptions<CronjobsOptions>>().Value;
            var listEndpointPattern = options.RoutePattern.TrimEnd('/');
            var triggerEndpointPattern = $"{options.RoutePattern.TrimEnd('/')}/{{name:required}}";

            endpoints.MapGet(listEndpointPattern, context =>
            {
                var providers = context.RequestServices.GetRequiredService<CronjobWebhookProvider>();
                var payload = JsonSerializer.Serialize(
                    providers.Cronjobs,
                    new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase}
                );
                context.Response.ContentType = MediaTypeNames.Application.Json;
                return context.Response.WriteAsync(payload);
            });

            return endpoints.MapPost(triggerEndpointPattern, async context =>
            {
                if (!(context.GetRouteValue("name") is string cronjobName))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("CronjobWebhook");
                if (!context.Request.Headers.TryGetValue("Execution-Id", out var executionId))
                {
                    logger.LogInformation("Request to trigger {Cronjob} is denied, because it's missing Execution-Id header", cronjobName);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                logger.LogInformation("Received request to trigger {Cronjob}", cronjobName);

                var factory = context.RequestServices.GetRequiredService<ICronjobFactory>();
                var cronjob = factory.Create(cronjobName);

                // Cronjobs will be executed outside request context,
                // So we're using endpoints.ServiceProvider, because context.RequestServices is request-scoped.
                var executionContext = new CronjobExecutionContext(cronjob, executionId, endpoints.ServiceProvider);

                var executorQueue = context.RequestServices.GetRequiredService<ICronjobQueue>();
                await executorQueue.EnqueueAsync(executionContext).ConfigureAwait(false);

                context.Response.StatusCode = StatusCodes.Status202Accepted;
            });
        }
    }
}