using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class ClaimSubmissionInputGraphType : InputObjectGraphType<ClaimSubmissionInput>
{
    public ClaimSubmissionInputGraphType()
    {
        Name = "ClaimSubmissionInput";
        Field<NonNullGraphType<IdGraphType>>("memberId");
        Field<NonNullGraphType<IdGraphType>>("providerId");
        Field<NonNullGraphType<GraphQL.Types.DateOnlyGraphType>>("serviceFrom");
        Field<NonNullGraphType<GraphQL.Types.DateOnlyGraphType>>("serviceTo");
        Field<NonNullGraphType<GraphQL.Types.DateOnlyGraphType>>("receivedDate");
        Field<NonNullGraphType<StringGraphType>>("idempotencyKey");
        Field<ListGraphType<NonNullGraphType<ClaimDiagnosisInputGraphType>>>("diagnoses");
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<ClaimLineInputGraphType>>>>("lines");
    }
}

public class ClaimSubmissionInput
{
    public Guid MemberId { get; set; }
    public Guid ProviderId { get; set; }
    public DateOnly ServiceFrom { get; set; }
    public DateOnly ServiceTo { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public List<ClaimDiagnosisInput>? Diagnoses { get; set; }
    public List<ClaimLineInput> Lines { get; set; } = null!;
}

