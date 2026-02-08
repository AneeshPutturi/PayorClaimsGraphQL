using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Loaders;
using PayorClaims.Schema.Scalars;
namespace PayorClaims.Schema.Types;

public class CoverageType : ObjectGraphType<Coverage>
{
    public CoverageType()
    {
        Name = "Coverage";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("memberId").Resolve(c => c.Source.MemberId);
        Field<NonNullGraphType<IdGraphType>>("planId").Resolve(c => c.Source.PlanId);
        Field<NonNullGraphType<DateOnlyGraphType>>("startDate").Resolve(c => c.Source.StartDate);
        Field<DateOnlyGraphType>("endDate").Resolve(c => c.Source.EndDate);
        Field<NonNullGraphType<StringGraphType>>("coverageStatus").Resolve(c => c.Source.CoverageStatus);

        Field<PlanType>("plan")
            .Resolve(c =>
            {
                var accessor = c.RequestServices!.GetRequiredService<IDataLoaderContextAccessor>();
                var loader = c.RequestServices!.GetRequiredService<PlanByIdLoader>();
                return loader.LoadAsync(c.Source.PlanId, accessor.Context!);
            });
    }
}
