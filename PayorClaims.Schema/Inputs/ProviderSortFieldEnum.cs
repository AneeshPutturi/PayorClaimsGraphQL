using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class ProviderSortFieldEnum : EnumerationGraphType<ProviderSortField>
{
    public ProviderSortFieldEnum()
    {
        Name = "ProviderSortField";
    }
}

public enum ProviderSortField { Name, Npi, Specialty, CreatedAt }
