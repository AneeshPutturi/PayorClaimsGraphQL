using System.Reactive.Linq;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Application.Events;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;
using PayorClaims.Schema.Types;

namespace PayorClaims.Schema.Schema;

public class AppSubscription : ObjectGraphType
{
    public AppSubscription()
    {
        Name = "Subscription";

        Field<NonNullGraphType<ClaimType>>("claimStatusChanged")
            .Argument<NonNullGraphType<IdGraphType>>("claimId")
            .ResolveStream(ResolveClaimStatusChanged);

        Field<NonNullGraphType<EobType>>("eobGenerated")
            .Argument<NonNullGraphType<IdGraphType>>("memberId")
            .ResolveStream(ResolveEobGenerated);
    }

    private IObservable<Claim> ResolveClaimStatusChanged(IResolveFieldContext context)
    {
        var claimId = context.GetArgument<Guid>("claimId");
        var eventBus = context.RequestServices!.GetRequiredService<IEventBus>();
        var sp = context.RequestServices!.GetRequiredService<IServiceProvider>();

        return eventBus.Subscribe<ClaimStatusChangedEvent>()
            .Where(e => e.ClaimId == claimId)
            .SelectMany(evt => Observable.FromAsync(async ct =>
            {
                await using var scope = sp.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
                return await db.Claims.AsNoTracking().FirstOrDefaultAsync(c => c.Id == evt.ClaimId, ct);
            }))
            .Where(c => c != null).Select(c => c!);
    }

    private IObservable<Eob> ResolveEobGenerated(IResolveFieldContext context)
    {
        var memberId = context.GetArgument<Guid>("memberId");
        var eventBus = context.RequestServices!.GetRequiredService<IEventBus>();
        var sp = context.RequestServices!.GetRequiredService<IServiceProvider>();

        return eventBus.Subscribe<EobGeneratedEvent>()
            .SelectMany(evt => Observable.FromAsync(async ct =>
            {
                await using var scope = sp.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
                var eob = await db.Eobs.AsNoTracking().Include(e => e.Claim).FirstOrDefaultAsync(e => e.Id == evt.EobId, ct);
                if (eob?.Claim?.MemberId == memberId) return eob;
                return null;
            }))
            .Where(e => e != null).Select(e => e!);
    }
}
