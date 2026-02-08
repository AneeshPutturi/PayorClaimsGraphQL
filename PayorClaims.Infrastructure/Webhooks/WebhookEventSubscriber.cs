using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayorClaims.Application.Abstractions;
using PayorClaims.Application.Events;

namespace PayorClaims.Infrastructure.Webhooks;

/// <summary>
/// Subscribes to domain events and enqueues webhook deliveries.
/// </summary>
public class WebhookEventSubscriber : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<WebhookEventSubscriber> _logger;

    public WebhookEventSubscriber(IServiceScopeFactory scopeFactory, IEventBus eventBus, ILogger<WebhookEventSubscriber> logger)
    {
        _scopeFactory = scopeFactory;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _eventBus.Subscribe<ClaimStatusChangedEvent>().Subscribe(evt =>
        {
            using var scope = _scopeFactory.CreateScope();
            var webhooks = scope.ServiceProvider.GetRequiredService<IWebhookService>();
            webhooks.EnqueueAsync("claim.statusChanged", new
            {
                claimId = evt.ClaimId,
                oldStatus = evt.OldStatus,
                newStatus = evt.NewStatus,
                changedAt = evt.ChangedAt
            }, stoppingToken).GetAwaiter().GetResult();
        });

        _eventBus.Subscribe<EobGeneratedEvent>().Subscribe(evt =>
        {
            using var scope = _scopeFactory.CreateScope();
            var webhooks = scope.ServiceProvider.GetRequiredService<IWebhookService>();
            webhooks.EnqueueAsync("eob.generated", new
            {
                eobId = evt.EobId,
                claimId = evt.ClaimId,
                generatedAt = evt.GeneratedAt
            }, stoppingToken).GetAwaiter().GetResult();
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
