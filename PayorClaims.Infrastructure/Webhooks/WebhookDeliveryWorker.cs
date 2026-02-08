using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Webhooks;

public class WebhookDeliveryWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private static readonly int MaxAttempts = 8;
    private static readonly TimeSpan[] Backoff = { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromHours(1), TimeSpan.FromHours(6), TimeSpan.FromHours(24) };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookDeliveryWorker> _logger;

    public WebhookDeliveryWorker(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, ILogger<WebhookDeliveryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingDeliveriesAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook delivery cycle failed");
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessPendingDeliveriesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var now = DateTime.UtcNow;
        var deliveries = await db.WebhookDeliveries
            .Where(d => d.Status == "Pending" && d.NextAttemptAt != null && d.NextAttemptAt <= now)
            .Take(50)
            .ToListAsync(ct);
        var endpointIds = deliveries.Select(d => d.WebhookEndpointId).Distinct().ToList();
        var endpoints = await db.WebhookEndpoints.Where(e => endpointIds.Contains(e.Id) && e.IsActive).ToDictionaryAsync(e => e.Id, ct);

        var client = _httpClientFactory.CreateClient();
        foreach (var d in deliveries)
        {
            if (!endpoints.TryGetValue(d.WebhookEndpointId, out var ep)) continue;
            var (succeeded, statusCode, error) = await SendAsync(client, ep, d, ct);
            d.LastAttemptAt = now;
            d.LastStatusCode = statusCode;
            d.LastError = error;
            d.AttemptCount++;
            if (succeeded)
            {
                d.Status = "Succeeded";
                d.NextAttemptAt = null;
            }
            else
            {
                if (d.AttemptCount >= MaxAttempts)
                    d.Status = "Failed";
                else
                {
                    var backoffIdx = Math.Min(d.AttemptCount - 1, Backoff.Length - 1);
                    d.NextAttemptAt = now.Add(Backoff[backoffIdx]);
                }
            }
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task<(bool Succeeded, int? StatusCode, string? Error)> SendAsync(HttpClient client, WebhookEndpoint ep, WebhookDelivery d, CancellationToken ct)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signedPayload = $"{timestamp}.{d.PayloadJson}";
            using var req = new HttpRequestMessage(HttpMethod.Post, ep.Url);
            req.Content = new StringContent(d.PayloadJson, Encoding.UTF8, "application/json");
            req.Headers.TryAddWithoutValidation("X-Webhook-EventType", d.EventType);
            req.Headers.TryAddWithoutValidation("X-Webhook-DeliveryId", d.Id.ToString());
            req.Headers.TryAddWithoutValidation("X-Webhook-Timestamp", timestamp);
            var signature = ComputeSignature(ep.Secret ?? "", signedPayload);
            req.Headers.TryAddWithoutValidation("X-Webhook-Signature", signature);

            var res = await client.SendAsync(req, ct);
            return (res.IsSuccessStatusCode, (int)res.StatusCode, res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync(ct));
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    /// <summary>Signature over "{timestamp}.{payloadJson}" for replay protection. HMACSHA256(secret, signedPayload) as hex.</summary>
    private static string ComputeSignature(string secret, string signedPayload)
    {
        var bytes = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(signedPayload));
        return "sha256=" + Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
