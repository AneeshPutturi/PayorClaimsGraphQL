using GraphQL.Types;

namespace PayorClaims.Schema.Types;

public class AdjudicateClaimPayloadType : ObjectGraphType<AdjudicateClaimPayload>
{
    public AdjudicateClaimPayloadType()
    {
        Name = "AdjudicateClaimPayload";
        Field<NonNullGraphType<ClaimType>>("claim").Resolve(c => c.Source.Claim);
    }
}

public class AdjudicateClaimPayload
{
    public PayorClaims.Domain.Entities.Claim Claim { get; set; } = null!;
}
