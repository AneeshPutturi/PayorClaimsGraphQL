using GraphQL.Types;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Schema.Types;

public class ProviderPageType : ObjectGraphType<ProviderPage>
{
    public ProviderPageType()
    {
        Name = "ProviderPage";
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<ProviderType>>>>("items").Resolve(c => c.Source.Items);
        Field<NonNullGraphType<IntGraphType>>("totalCount").Resolve(c => c.Source.TotalCount);
        Field<NonNullGraphType<IntGraphType>>("skip").Resolve(c => c.Source.Skip);
        Field<NonNullGraphType<IntGraphType>>("take").Resolve(c => c.Source.Take);
    }
}

public class ProviderPage
{
    public List<Provider> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
