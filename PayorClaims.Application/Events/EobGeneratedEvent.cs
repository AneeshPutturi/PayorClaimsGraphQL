namespace PayorClaims.Application.Events;

public record EobGeneratedEvent(Guid EobId, Guid ClaimId, DateTime GeneratedAt);
