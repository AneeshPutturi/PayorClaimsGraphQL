using Microsoft.AspNetCore.Http;
using PayorClaims.Application.Security;

namespace PayorClaims.Api.Middleware;

public class ActorContextMiddleware
{
    public static readonly string ActorContextKey = PayorClaims.Application.Security.ActorContext.HttpContextItemKey;

    private readonly RequestDelegate _nextActual;

    public ActorContextMiddleware(RequestDelegate next) => _nextActual = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var actor = new ActorContext();
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sub = context.User.FindFirst("sub")?.Value;
            if (Guid.TryParse(sub, out var actorId))
                actor.ActorId = actorId;

            var roles = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                ?? context.User.FindFirst("role")?.Value;
            if (!string.IsNullOrEmpty(roles))
                actor.Roles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            actor.Npi = context.User.FindFirst("npi")?.Value;
            var memberIdStr = context.User.FindFirst("memberId")?.Value;
            if (Guid.TryParse(memberIdStr, out var memberId))
                actor.MemberId = memberId;

            if (actor.Roles.Contains("Admin")) actor.ActorType = ActorContext.AdminActorType;
            else if (actor.Roles.Contains("Adjuster")) actor.ActorType = ActorContext.AdjusterActorType;
            else if (actor.Roles.Contains("Provider")) actor.ActorType = ActorContext.ProviderActorType;
            else if (actor.Roles.Contains("Member")) actor.ActorType = ActorContext.MemberActorType;
        }

        context.Items[PayorClaims.Application.Security.ActorContext.HttpContextItemKey] = actor;
        await _nextActual(context);
    }
}

public static class ActorContextHttpContextExtensions
{
    public static ActorContext GetActorContext(this HttpContext context)
    {
        return (context.Items[ActorContext.HttpContextItemKey] as ActorContext) ?? new ActorContext();
    }
}
