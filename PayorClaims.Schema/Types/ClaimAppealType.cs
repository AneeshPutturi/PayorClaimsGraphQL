using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;

namespace PayorClaims.Schema.Types;

public class ClaimAppealType : ObjectGraphType<ClaimAppeal>
{
    public ClaimAppealType()
    {
        Name = "ClaimAppeal";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("claimId").Resolve(c => c.Source.ClaimId);
        Field<NonNullGraphType<IntGraphType>>("appealLevel").Resolve(c => c.Source.AppealLevel);
        Field<NonNullGraphType<StringGraphType>>("reason").Resolve(c => c.Source.Reason);
        Field<NonNullGraphType<StringGraphType>>("status").Resolve(c => c.Source.Status);
        Field<NonNullGraphType<DateTimeGraphType>>("submittedAt").Resolve(c => c.Source.SubmittedAt);
        Field<DateTimeGraphType>("decisionAt").Resolve(c => c.Source.DecisionAt);
    }
}
