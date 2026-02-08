using GraphQL.Types;

namespace PayorClaims.Schema.Types.Actors;

public class SystemProcessType : ObjectGraphType<SystemActorDto>
{
    public SystemProcessType()
    {
        Name = "SystemProcess";
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<StringGraphType>>("name").Resolve(c => c.Source.Name);
    }
}
