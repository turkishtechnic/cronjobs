using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TT.Cronjobs.Blitz
{
    class BlitzCronjobApiClient : ICronjobApiClient
    {
        private readonly ILogger<BlitzCronjobApiClient> _logger;
        private readonly HttpClient _httpClient;

        public BlitzCronjobApiClient(HttpClient httpClient, ILogger<BlitzCronjobApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task BatchRegisterProjectAsync(ProjectBatchRegistration registration,
                                                    CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(registration);
            var res = await _httpClient.PostAsync(
                "projects/batchcreate",
                new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json),
                cancellationToken: cancellationToken
            );
            res.EnsureSuccessStatusCode();
        }

        public async Task UpdateExecutionStatusAsync(Guid executionId,
                                                     ExecutionState state,
                                                     Dictionary<string, object> details = null,
                                                     CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending stating update for execution {ExecutionId}", executionId);
            var serialized = JsonSerializer.Serialize(new
            {
                State = state.Name,
                Details = details
            });
            var res = await _httpClient.PostAsync(
                $"executions/{executionId}/status",
                new StringContent(serialized, Encoding.UTF8, MediaTypeNames.Application.Json),
                cancellationToken
            );
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Cannot send status update for execution {ExecutionId}. API returned {StatusCode}",
                    executionId, (int) res.StatusCode);
            }
        }
    }
}