using GraphQL.Types;

namespace PayorClaims.Schema.Types;

public class SubmitClaimPayloadType : ObjectGraphType<SubmitClaimPayload>
{
    public SubmitClaimPayloadType()
    {
        Name = "SubmitClaimPayload";
        Field<NonNullGraphType<ClaimType>>("claim").Resolve(c => c.Source.Claim);
        Field<NonNullGraphType<BooleanGraphType>>("alreadyExisted").Resolve(c => c.Source.AlreadyExisted);
    }
}

public class SubmitClaimPayload
{
    public PayorClaims.Domain.Entities.Claim Claim { get; set; } = null!;
    public bool AlreadyExisted { get; set; }
}
