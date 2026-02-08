using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class ClaimSortFieldEnum : EnumerationGraphType<ClaimSortField>
{
    public ClaimSortFieldEnum()
    {
        Name = "ClaimSortField";
    }
}

public enum ClaimSortField { ReceivedDate, Status, TotalBilled, CreatedAt }
