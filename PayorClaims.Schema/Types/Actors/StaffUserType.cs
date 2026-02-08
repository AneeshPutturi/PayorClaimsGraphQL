using GraphQL.Types;

namespace PayorClaims.Schema.Types.Actors;

public class StaffUserType : ObjectGraphType<StaffActorDto>
{
    public StaffUserType()
    {
        Name = "StaffUser";
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<StringGraphType>>("displayName").Resolve(c => c.Source.DisplayName);
        Field<NonNullGraphType<StringGraphType>>("role").Resolve(c => c.Source.Role);
    }
}
