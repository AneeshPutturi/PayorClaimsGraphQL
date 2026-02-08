using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Types;

public class AccumulatorType : ObjectGraphType<Accumulator>
{
    public AccumulatorType()
    {
        Name = "Accumulator";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("memberId").Resolve(c => c.Source.MemberId);
        Field<NonNullGraphType<IdGraphType>>("planId").Resolve(c => c.Source.PlanId);
        Field<NonNullGraphType<IntGraphType>>("year").Resolve(c => c.Source.Year);
        Field<NonNullGraphType<StringGraphType>>("network").Resolve(c => c.Source.Network);
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("deductibleMet").Resolve(c => c.Source.DeductibleMet);
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("moopMet").Resolve(c => c.Source.MoopMet);
    }
}
