using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class ClaimDiagnosisInputGraphType : InputObjectGraphType<ClaimDiagnosisInput>
{
    public ClaimDiagnosisInputGraphType()
    {
        Name = "ClaimDiagnosisInput";
        Field<NonNullGraphType<StringGraphType>>("codeSystem");
        Field<NonNullGraphType<StringGraphType>>("code");
        Field<NonNullGraphType<BooleanGraphType>>("isPrimary");
        Field<IntGraphType>("lineNumber");
    }
}

public class ClaimDiagnosisInput
{
    public string CodeSystem { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int? LineNumber { get; set; }
}
