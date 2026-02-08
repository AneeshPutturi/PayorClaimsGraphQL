using GraphQL;
using Microsoft.Extensions.DependencyInjection;

namespace PayorClaims.Schema.Schema;

public class AppSchema : GraphQL.Types.Schema
{
    public AppSchema(IServiceProvider sp)
    {
        Query = sp.GetRequiredService<AppQuery>();
        Mutation = sp.GetRequiredService<AppMutation>();
        Subscription = sp.GetRequiredService<AppSubscription>();
    }
}
