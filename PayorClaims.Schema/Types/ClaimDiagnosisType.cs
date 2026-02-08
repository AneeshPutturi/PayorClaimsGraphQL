using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Types;

public class ClaimDiagnosisType : ObjectGraphType<ClaimDiagnosis>
{
    public ClaimDiagnosisType()
    {
        Name = "ClaimDiagnosis";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("claimId").Resolve(c => c.Source.ClaimId);
        Field<IntGraphType>("lineNumber").Resolve(c => c.Source.LineNumber);
        Field<NonNullGraphType<StringGraphType>>("codeSystem").Resolve(c => c.Source.CodeSystem);
        Field<NonNullGraphType<StringGraphType>>("code").Resolve(c => c.Source.Code);
        Field<NonNullGraphType<BooleanGraphType>>("isPrimary").Resolve(c => c.Source.IsPrimary);
    }
}
