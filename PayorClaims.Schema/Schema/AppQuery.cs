using GraphQL.Types;

namespace PayorClaims.Schema.Schema;

public class AppQuery : ObjectGraphType
{
    public AppQuery()
    {
        Field<StringGraphType>("ping")
            .Resolve(_ => "pong");
    }
}
