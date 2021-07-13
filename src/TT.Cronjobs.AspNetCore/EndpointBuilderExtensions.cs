using System;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
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
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );
                context.Response.ContentType = MediaTypeNames.Application.Json;
                return context.Response.WriteAsync(payload);
            });

            return endpoints.MapPost(triggerEndpointPattern, async context =>
            {
                // We're not using IEndpointConventionsBuilder.RequireAuthorization method
                // because we're not sure if the authentication services are registered
                // and don't want to throw error
                var authResult = await context.RequestServices.GetRequiredService<IAuthorizationService>().AuthorizeAsync(context.User, "cronjob");
                if (!authResult.Succeeded)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

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


                ICronjob cronjob;
                try
                {
                    var factory = context.RequestServices.GetRequiredService<ICronjobFactory>();
                    cronjob = factory.Create(cronjobName);
                }
                catch (ApplicationException e)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                logger.LogInformation("Received request to trigger {CronjobType}", cronjob.GetType().FullName);

                // Cronjobs will be executed outside request context,
                // So we're using endpoints.ServiceProvider, because context.RequestServices is request-scoped.
                var executionContext = new CronjobExecutionContext(cronjob, executionId);

                var executorQueue = context.RequestServices.GetRequiredService<ICronjobQueue>();
                await executorQueue.EnqueueAsync(executionContext).ConfigureAwait(false);

                context.Response.StatusCode = StatusCodes.Status202Accepted;
            });
        }
    }
}