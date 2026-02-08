using Microsoft.AspNetCore.Http;
using PayorClaims.Application.Security;

namespace PayorClaims.Api.Security;

public class ActorContextProvider : IActorContextProvider
{
    private readonly IHttpContextAccessor _accessor;

    public ActorContextProvider(IHttpContextAccessor accessor) => _accessor = accessor;

    public ActorContext GetActorContext()
    {
        var ctx = _accessor.HttpContext;
        if (ctx?.Items[ActorContext.HttpContextItemKey] is ActorContext actor)
            return actor;
        return new ActorContext();
    }
}
