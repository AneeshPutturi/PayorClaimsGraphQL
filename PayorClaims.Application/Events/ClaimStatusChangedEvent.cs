namespace PayorClaims.Application.Events;

public record ClaimStatusChangedEvent(Guid ClaimId, string OldStatus, string NewStatus, DateTime ChangedAt);
