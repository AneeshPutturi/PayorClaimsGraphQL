using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Types;

public class MemberConsentType : ObjectGraphType<MemberConsent>
{
    public MemberConsentType()
    {
        Name = "MemberConsent";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("memberId").Resolve(c => c.Source.MemberId);
        Field<NonNullGraphType<StringGraphType>>("consentType").Resolve(c => c.Source.ConsentType);
        Field<NonNullGraphType<BooleanGraphType>>("granted").Resolve(c => c.Source.Granted);
        Field<NonNullGraphType<DateTimeGraphType>>("grantedAt").Resolve(c => c.Source.GrantedAt);
        Field<DateTimeGraphType>("revokedAt").Resolve(c => c.Source.RevokedAt);
        Field<NonNullGraphType<StringGraphType>>("source").Resolve(c => c.Source.Source);
    }
}
