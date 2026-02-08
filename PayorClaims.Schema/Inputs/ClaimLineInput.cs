using GraphQL.Types;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Inputs;

public class ClaimLineInputGraphType : InputObjectGraphType<ClaimLineInput>
{
    public ClaimLineInputGraphType()
    {
        Name = "ClaimLineInput";
        Field<NonNullGraphType<IntGraphType>>("lineNumber");
        Field<NonNullGraphType<StringGraphType>>("cptCode");
        Field<NonNullGraphType<IntGraphType>>("units");
        Field<NonNullGraphType<DecimalGraphType>>("billedAmount");
        Field<ListGraphType<NonNullGraphType<ClaimDiagnosisInputGraphType>>>("diagnoses");
    }
}

public class ClaimLineInput
{
    public int LineNumber { get; set; }
    public string CptCode { get; set; } = null!;
    public int Units { get; set; }
    public decimal BilledAmount { get; set; }
    public List<ClaimDiagnosisInput>? Diagnoses { get; set; }
}
