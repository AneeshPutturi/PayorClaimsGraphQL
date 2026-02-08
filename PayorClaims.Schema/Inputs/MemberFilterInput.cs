using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class MemberFilterInputType : InputObjectGraphType<MemberFilterInput>
{
    public MemberFilterInputType()
    {
        Name = "MemberFilterInput";
        Field<StringGraphType>("status");
        Field<StringGraphType>("nameContains");
        Field<DateOnlyGraphType>("dobFrom");
        Field<DateOnlyGraphType>("dobTo");
    }
}

public class MemberFilterInput
{
    public string? Status { get; set; }
    public string? NameContains { get; set; }
    public DateOnly? DobFrom { get; set; }
    public DateOnly? DobTo { get; set; }
}
