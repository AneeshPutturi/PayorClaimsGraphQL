using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class ProviderFilterInputType : InputObjectGraphType<ProviderFilterInput>
{
    public ProviderFilterInputType()
    {
        Name = "ProviderFilterInput";
        Field<StringGraphType>("npi");
        Field<StringGraphType>("nameContains");
        Field<StringGraphType>("specialty");
        Field<StringGraphType>("status");
    }
}

public class ProviderFilterInput
{
    public string? Npi { get; set; }
    public string? NameContains { get; set; }
    public string? Specialty { get; set; }
    public string? Status { get; set; }
}
