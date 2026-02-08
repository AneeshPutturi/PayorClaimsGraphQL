using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class AdjudicateLineInputGraphType : InputObjectGraphType<AdjudicateLineInput>
{
    public AdjudicateLineInputGraphType()
    {
        Name = "AdjudicateLineInput";
        Field<NonNullGraphType<IntGraphType>>("lineNumber");
        Field<NonNullGraphType<StringGraphType>>("status");
        Field<StringGraphType>("denialReasonCode");
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("allowedAmount");
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("paidAmount");
    }
}

public class AdjudicateLineInput
{
    public int LineNumber { get; set; }
    public string Status { get; set; } = null!;
    public string? DenialReasonCode { get; set; }
    public decimal AllowedAmount { get; set; }
    public decimal PaidAmount { get; set; }
}
