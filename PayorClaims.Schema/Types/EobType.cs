using GraphQL.Types;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Schema.Types;

public class EobType : ObjectGraphType<Eob>
{
    public EobType()
    {
        Name = "Eob";
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<IdGraphType>>("claimId").Resolve(c => c.Source.ClaimId);
        Field<NonNullGraphType<StringGraphType>>("eobNumber").Resolve(c => c.Source.EobNumber);
        Field<NonNullGraphType<DateTimeGraphType>>("generatedAt").Resolve(c => c.Source.GeneratedAt);
        Field<NonNullGraphType<StringGraphType>>("documentStorageKey").Resolve(c => c.Source.DocumentStorageKey);
        Field<NonNullGraphType<StringGraphType>>("deliveryStatus").Resolve(c => c.Source.DeliveryStatus);
    }
}
