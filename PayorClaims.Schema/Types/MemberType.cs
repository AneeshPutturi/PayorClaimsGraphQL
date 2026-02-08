using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Loaders;
using PayorClaims.Schema;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Schema.Types;

public class MemberType : ObjectGraphType<Member>
{
    public MemberType()
    {
        Name = "Member";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<StringGraphType>>("externalMemberNumber").Resolve(c => c.Source.ExternalMemberNumber);
        Field<NonNullGraphType<StringGraphType>>("firstName").Resolve(c => c.Source.FirstName);
        Field<NonNullGraphType<StringGraphType>>("lastName").Resolve(c => c.Source.LastName);
        Field<NonNullGraphType<DateOnlyGraphType>>("dob").Resolve(c => c.Source.Dob);
        Field<StringGraphType>("gender").Resolve(c => c.Source.Gender);
        Field<NonNullGraphType<StringGraphType>>("status").Resolve(c => c.Source.Status);

        Field<ListGraphType<NonNullGraphType<CoverageType>>>("coverages")
            .Resolve(c =>
            {
                var accessor = c.RequestServices!.GetRequiredService<IDataLoaderContextAccessor>();
                var loader = c.RequestServices!.GetRequiredService<CoveragesByMemberIdLoader>();
                return loader.LoadAsync(c.Source.Id, accessor.Context!);
            });

        Field<ListGraphType<NonNullGraphType<ClaimType>>>("recentClaims")
            .Argument<IntGraphType>("limit")
            .ResolveAsync(async c =>
            {
                var limit = Math.Min(Math.Max(1, c.GetArgument("limit", 10)), 100);
                var db = c.RequestServices!.GetRequiredService<ClaimsDbContext>();
                return await db.Claims
                    .Where(claim => claim.MemberId == c.Source.Id)
                    .OrderByDescending(claim => claim.ReceivedDate)
                    .Take(limit)
                    .ToListAsync(c.CancellationToken);
            });

        Field<CoverageType>("activeCoverage")
            .Argument<NonNullGraphType<DateOnlyGraphType>>("asOf")
            .Resolve(c =>
            {
                var asOf = c.GetArgument<DateOnly>("asOf");
                var accessor = c.RequestServices!.GetRequiredService<IDataLoaderContextAccessor>();
                var loader = c.RequestServices!.GetRequiredService<CoveragesByMemberIdLoader>();
                return loader.LoadAsync(c.Source.Id, accessor.Context!).Then(coverages => coverages?.FirstOrDefault(co =>
                    co.StartDate <= asOf && (co.EndDate == null || co.EndDate >= asOf) && co.CoverageStatus == "Active"));
            });

        Field<PlanType>("activePlan")
            .Argument<NonNullGraphType<DateOnlyGraphType>>("asOf")
            .Resolve(c =>
            {
                var asOf = c.GetArgument<DateOnly>("asOf");
                var accessor = c.RequestServices!.GetRequiredService<IDataLoaderContextAccessor>();
                var covLoader = c.RequestServices!.GetRequiredService<CoveragesByMemberIdLoader>();
                var planLoader = c.RequestServices!.GetRequiredService<PlanByIdLoader>();
                return covLoader.LoadAsync(c.Source.Id, accessor.Context!).Then(coverages =>
                {
                    var active = coverages?.FirstOrDefault(co =>
                        co.StartDate <= asOf && (co.EndDate == null || co.EndDate >= asOf) && co.CoverageStatus == "Active");
                    return planLoader.LoadAsync(active?.PlanId ?? Guid.Empty, accessor.Context!);
                });
            });
    }
}
