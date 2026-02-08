using GraphQL.Types;

namespace PayorClaims.Schema.Types.Actors;

public class ProviderUserType : ObjectGraphType<ProviderActorDto>
{
    public ProviderUserType()
    {
        Name = "ProviderUser";
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<StringGraphType>>("npi").Resolve(c => c.Source.Npi);
        Field<NonNullGraphType<StringGraphType>>("name").Resolve(c => c.Source.Name);
    }
}
