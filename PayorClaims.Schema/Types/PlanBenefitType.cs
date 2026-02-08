using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Types;

public class PlanBenefitType : ObjectGraphType<PlanBenefit>
{
    public PlanBenefitType()
    {
        Name = "PlanBenefit";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("planId").Resolve(c => c.Source.PlanId);
        Field<NonNullGraphType<IntGraphType>>("benefitVersion").Resolve(c => c.Source.BenefitVersion);
        Field<NonNullGraphType<DateOnlyGraphType>>("effectiveFrom").Resolve(c => c.Source.EffectiveFrom);
        Field<DateOnlyGraphType>("effectiveTo").Resolve(c => c.Source.EffectiveTo);
        Field<NonNullGraphType<StringGraphType>>("category").Resolve(c => c.Source.Category);
        Field<NonNullGraphType<StringGraphType>>("network").Resolve(c => c.Source.Network);
        Field<NonNullGraphType<StringGraphType>>("coverageLevel").Resolve(c => c.Source.CoverageLevel);
        Field<NonNullGraphType<StringGraphType>>("period").Resolve(c => c.Source.Period);
        Field<PayorClaims.Schema.Scalars.DecimalGraphType>("copayAmount").Resolve(c => c.Source.CopayAmount);
        Field<PayorClaims.Schema.Scalars.DecimalGraphType>("coinsurancePercent").Resolve(c => c.Source.CoinsurancePercent);
        Field<NonNullGraphType<BooleanGraphType>>("deductibleApplies").Resolve(c => c.Source.DeductibleApplies);
        Field<IntGraphType>("maxVisits").Resolve(c => c.Source.MaxVisits);
        Field<StringGraphType>("notes").Resolve(c => c.Source.Notes);
    }
}
