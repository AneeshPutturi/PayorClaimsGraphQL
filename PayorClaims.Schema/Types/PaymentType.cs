using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Types;

public class PaymentType : ObjectGraphType<Payment>
{
    public PaymentType()
    {
        Name = "Payment";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("claimId").Resolve(c => c.Source.ClaimId);
        Field<NonNullGraphType<DateTimeGraphType>>("paymentDate").Resolve(c => c.Source.PaymentDate);
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("amount").Resolve(c => c.Source.Amount);
        Field<NonNullGraphType<StringGraphType>>("method").Resolve(c => c.Source.Method);
        Field<StringGraphType>("referenceNumber").Resolve(c => c.Source.ReferenceNumber);
        Field<StringGraphType>("idempotencyKey").Resolve(c => c.Source.IdempotencyKey);
    }
}
