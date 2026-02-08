namespace PayorClaims.Application.Security;

/// <summary>
/// Provides the current request's actor context (from JWT / HttpContext).
/// Implemented in the API layer; used by Schema resolvers.
/// </summary>
public interface IActorContextProvider
{
    ActorContext GetActorContext();
}
