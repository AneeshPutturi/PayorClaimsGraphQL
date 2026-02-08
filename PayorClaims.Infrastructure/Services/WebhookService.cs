using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Application.Abstractions;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly ClaimsDbContext _db;

    public WebhookService(ClaimsDbContext db)
    {
        _db = db;
    }

    public async Task<WebhookEndpoint> RegisterEndpointAsync(string name, string url, string? secret, CancellationToken ct = default)
    {
        var endpoint = new WebhookEndpoint
        {
            Id = Guid.NewGuid(),
            Name = name,
            Url = url,
            Secret = secret,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.WebhookEndpoints.Add(endpoint);
        await _db.SaveChangesAsync(ct);
        return endpoint;
    }

    public async Task<WebhookEndpoint?> DeactivateEndpointAsync(Guid id, CancellationToken ct = default)
    {
        var endpoint = await _db.WebhookEndpoints.FindAsync([id], ct);
        if (endpoint == null) return null;
        endpoint.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return endpoint;
    }

    public async Task EnqueueAsync(string eventType, object payload, CancellationToken ct = default)
    {
        var active = await _db.WebhookEndpoints.Where(e => e.IsActive).ToListAsync(ct);
        var payloadJson = JsonSerializer.Serialize(payload);
        var now = DateTime.UtcNow;
        foreach (var ep in active)
        {
            _db.WebhookDeliveries.Add(new WebhookDelivery
            {
                Id = Guid.NewGuid(),
                WebhookEndpointId = ep.Id,
                EventType = eventType,
                PayloadJson = payloadJson,
                AttemptCount = 0,
                NextAttemptAt = now,
                Status = "Pending"
            });
        }
        await _db.SaveChangesAsync(ct);
    }
}
