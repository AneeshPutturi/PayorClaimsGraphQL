using GraphQL.Types;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Schema.Types;

public class MemberPageType : ObjectGraphType<MemberPage>
{
    public MemberPageType()
    {
        Name = "MemberPage";
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<MemberType>>>>("items").Resolve(c => c.Source.Items);
        Field<NonNullGraphType<IntGraphType>>("totalCount").Resolve(c => c.Source.TotalCount);
        Field<NonNullGraphType<IntGraphType>>("skip").Resolve(c => c.Source.Skip);
        Field<NonNullGraphType<IntGraphType>>("take").Resolve(c => c.Source.Take);
    }
}

public class MemberPage
{
    public List<Member> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
