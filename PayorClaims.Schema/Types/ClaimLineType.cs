using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Types;

public class ClaimLineType : ObjectGraphType<ClaimLine>
{
    public ClaimLineType()
    {
        Name = "ClaimLine";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("claimId").Resolve(c => c.Source.ClaimId);
        Field<NonNullGraphType<IntGraphType>>("lineNumber").Resolve(c => c.Source.LineNumber);
        Field<NonNullGraphType<StringGraphType>>("cptCode").Resolve(c => c.Source.CptCode);
        Field<NonNullGraphType<IntGraphType>>("units").Resolve(c => c.Source.Units);
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("billedAmount").Resolve(c => c.Source.BilledAmount);
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("allowedAmount").Resolve(c => c.Source.AllowedAmount);
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("paidAmount").Resolve(c => c.Source.PaidAmount);
        Field<NonNullGraphType<StringGraphType>>("lineStatus").Resolve(c => c.Source.LineStatus);
        Field<StringGraphType>("denialReasonCode").Resolve(c => c.Source.DenialReasonCode);
        Field<StringGraphType>("denialReasonText").Resolve(c => c.Source.DenialReasonText);
    }
}
