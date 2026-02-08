using GraphQL.Types;

namespace PayorClaims.Schema.Inputs;

public class MemberSortFieldEnum : EnumerationGraphType<MemberSortField>
{
    public MemberSortFieldEnum()
    {
        Name = "MemberSortField";
    }
}

public enum MemberSortField { LastName, FirstName, Dob, CreatedAt }
