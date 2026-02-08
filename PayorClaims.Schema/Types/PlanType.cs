using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Loaders;
using PayorClaims.Schema.Scalars;
using PayorClaims.Schema;
namespace PayorClaims.Schema.Types;

public class PlanType : ObjectGraphType<Plan>
{
    public PlanType()
    {
        Name = "Plan";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<StringGraphType>>("planCode").Resolve(c => c.Source.PlanCode);
        Field<NonNullGraphType<StringGraphType>>("name").Resolve(c => c.Source.Name);
        Field<NonNullGraphType<IntGraphType>>("year").Resolve(c => c.Source.Year);
        Field<NonNullGraphType<StringGraphType>>("networkType").Resolve(c => c.Source.NetworkType);
        Field<NonNullGraphType<StringGraphType>>("metalTier").Resolve(c => c.Source.MetalTier);
        Field<NonNullGraphType<BooleanGraphType>>("isActive").Resolve(c => c.Source.IsActive);

        Field<ListGraphType<NonNullGraphType<PlanBenefitType>>>("effectiveBenefits")
            .Argument<NonNullGraphType<DateOnlyGraphType>>("asOf")
            .Resolve(c =>
            {
                var asOf = c.GetArgument<DateOnly>("asOf");
                var accessor = c.RequestServices!.GetRequiredService<IDataLoaderContextAccessor>();
                var loader = c.RequestServices!.GetRequiredService<EffectiveBenefitsByPlanIdLoader>();
                return loader.LoadAsync(c.Source.Id, accessor.Context!).Then(benefits => (benefits ?? Enumerable.Empty<PlanBenefit>()).Where(b => b.EffectiveFrom <= asOf && (b.EffectiveTo == null || b.EffectiveTo >= asOf)).ToList());
            });
    }
}
