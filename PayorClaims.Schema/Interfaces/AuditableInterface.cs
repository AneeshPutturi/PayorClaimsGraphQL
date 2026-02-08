using GraphQL.Types;

namespace PayorClaims.Schema.Interfaces;

public class AuditableInterface : InterfaceGraphType
{
    public AuditableInterface()
    {
        Name = "IAuditableEntity";
        Description = "Entity with audit timestamps.";
        Field<NonNullGraphType<IdGraphType>>("id").Description("Unique identifier.");
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Description("Creation timestamp (UTC).");
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Description("Last update timestamp (UTC).");
    }
}
