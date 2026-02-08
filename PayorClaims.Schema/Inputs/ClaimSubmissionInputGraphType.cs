using GraphQL.Types;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Inputs;

public class ClaimSubmissionInputGraphType : InputObjectGraphType<ClaimSubmissionInput>
{
    public ClaimSubmissionInputGraphType()
    {
        Name = "ClaimSubmissionInput";
        Field<NonNullGraphType<IdGraphType>>("memberId");
        Field<NonNullGraphType<IdGraphType>>("providerId");
        Field<NonNullGraphType<DateOnlyGraphType>>("serviceFrom");
        Field<NonNullGraphType<DateOnlyGraphType>>("serviceTo");
        Field<NonNullGraphType<DateOnlyGraphType>>("receivedDate");
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

