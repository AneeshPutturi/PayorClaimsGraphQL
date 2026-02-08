using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class ClaimFilterInputType : InputObjectGraphType<ClaimFilterInput>
{
    public ClaimFilterInputType()
    {
        Name = "ClaimFilterInput";
        Field<IdGraphType>("memberId");
        Field<IdGraphType>("providerId");
        Field<StringGraphType>("status");
        Field<DateOnlyGraphType>("receivedFrom");
        Field<DateOnlyGraphType>("receivedTo");
        Field<StringGraphType>("claimNumber");
    }
}

public class ClaimFilterInput
{
    public Guid? MemberId { get; set; }
    public Guid? ProviderId { get; set; }
    public string? Status { get; set; }
    public DateOnly? ReceivedFrom { get; set; }
    public DateOnly? ReceivedTo { get; set; }
    public string? ClaimNumber { get; set; }
}
