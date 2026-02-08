using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class SortDirectionEnum : EnumerationGraphType<SortDirection>
{
    public SortDirectionEnum()
    {
        Name = "SortDirection";
    }
}

public enum SortDirection { ASC, DESC }
