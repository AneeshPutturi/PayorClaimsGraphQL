using PayorClaims.Domain.Entities;

namespace PayorClaims.Application.Abstractions;

public interface IWebhookService
{
    Task<WebhookEndpoint> RegisterEndpointAsync(string name, string url, string? secret, CancellationToken ct = default);
    Task<WebhookEndpoint?> DeactivateEndpointAsync(Guid id, CancellationToken ct = default);
    Task EnqueueAsync(string eventType, object payload, CancellationToken ct = default);
}
