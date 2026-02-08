using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Types;

public class PriorAuthType : ObjectGraphType<PriorAuth>
{
    public PriorAuthType()
    {
        Name = "PriorAuth";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("memberId").Resolve(c => c.Source.MemberId);
        Field<IdGraphType>("providerId").Resolve(c => c.Source.ProviderId);
        Field<NonNullGraphType<StringGraphType>>("serviceType").Resolve(c => c.Source.ServiceType);
        Field<NonNullGraphType<DateTimeGraphType>>("requestedDate").Resolve(c => c.Source.RequestedDate);
        Field<DateTimeGraphType>("decisionDate").Resolve(c => c.Source.DecisionDate);
        Field<NonNullGraphType<StringGraphType>>("status").Resolve(c => c.Source.Status);
        Field<StringGraphType>("notes").Resolve(c => c.Source.Notes);
    }
}
