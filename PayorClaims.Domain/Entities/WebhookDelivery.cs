namespace PayorClaims.Domain.Entities;

public class WebhookDelivery
{
    public Guid Id { get; set; }
    public Guid WebhookEndpointId { get; set; }
    public string EventType { get; set; } = null!;
    public string PayloadJson { get; set; } = null!;
    public int AttemptCount { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public int? LastStatusCode { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Succeeded, Failed
    public string? LastError { get; set; }
}
