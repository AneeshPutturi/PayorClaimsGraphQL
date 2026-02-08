using GraphQL.Types;
using PayorClaims.Schema.Types.Actors;

namespace PayorClaims.Schema.Unions;

public class ActorUnion : UnionGraphType
{
    public ActorUnion()
    {
        Name = "Actor";
        Description = "Actor that performed an action (staff, provider, or system).";
        Type<StaffUserType>();
        Type<ProviderUserType>();
        Type<SystemProcessType>();
    }
}
