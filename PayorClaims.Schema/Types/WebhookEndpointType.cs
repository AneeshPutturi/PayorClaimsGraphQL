using GraphQL.Types;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Schema.Types;

public class WebhookEndpointType : ObjectGraphType<WebhookEndpoint>
{
    public WebhookEndpointType()
    {
        Name = "WebhookEndpoint";
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source!.Id);
        Field<NonNullGraphType<StringGraphType>>("name").Resolve(c => c.Source!.Name);
        Field<NonNullGraphType<StringGraphType>>("url").Resolve(c => c.Source!.Url);
        Field<NonNullGraphType<BooleanGraphType>>("isActive").Resolve(c => c.Source!.IsActive);
        Field<NonNullGraphType<DateTimeOffsetGraphType>>("createdAt").Resolve(c => c.Source!.CreatedAt);
    }
}
