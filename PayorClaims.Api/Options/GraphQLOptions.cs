namespace PayorClaims.Api.Options;

public class GraphQLOptions
{
    public const string SectionName = "GraphQL";
    public int MaxDepth { get; set; } = 12;
    public int MaxComplexity { get; set; } = 2000;
    public bool PersistedQueriesEnabled { get; set; }
}
