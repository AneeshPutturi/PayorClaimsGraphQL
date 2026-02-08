namespace PayorClaims.Application.Abstractions;

public interface IAccessLogger
{
    Task LogReadAsync(string actorType, Guid? actorId, string subjectType, Guid subjectId, string purpose, CancellationToken ct = default);
}
