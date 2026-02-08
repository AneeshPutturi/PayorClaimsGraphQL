using GraphQL.Types;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Schema.Types;

public class ClaimPageType : ObjectGraphType<ClaimPage>
{
    public ClaimPageType()
    {
        Name = "ClaimPage";
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<ClaimType>>>>("items").Resolve(c => c.Source.Items);
        Field<NonNullGraphType<IntGraphType>>("totalCount").Resolve(c => c.Source.TotalCount);
        Field<NonNullGraphType<IntGraphType>>("skip").Resolve(c => c.Source.Skip);
        Field<NonNullGraphType<IntGraphType>>("take").Resolve(c => c.Source.Take);
    }
}

public class ClaimPage
{
    public List<Claim> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
