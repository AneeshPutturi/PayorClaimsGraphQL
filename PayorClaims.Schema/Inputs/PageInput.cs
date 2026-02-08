using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class PageInputType : InputObjectGraphType<PageInput>
{
    public PageInputType()
    {
        Name = "PageInput";
        Field<IntGraphType>("skip");
        Field<IntGraphType>("take");
    }
}

public class PageInput
{
    public int Skip { get; set; }
    public int Take { get; set; } = 25;
}
