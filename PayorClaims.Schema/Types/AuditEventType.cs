using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;
using PayorClaims.Schema.Types.Actors;
using PayorClaims.Schema.Unions;

namespace PayorClaims.Schema.Types;

public class AuditEventType : ObjectGraphType<AuditEvent>
{
    public AuditEventType()
    {
        Name = "AuditEvent";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<StringGraphType>>("actorUserId").Resolve(c => c.Source.ActorUserId);
        Field<NonNullGraphType<StringGraphType>>("action").Resolve(c => c.Source.Action);
        Field<NonNullGraphType<StringGraphType>>("entityType").Resolve(c => c.Source.EntityType);
        Field<NonNullGraphType<IdGraphType>>("entityId").Resolve(c => c.Source.EntityId);
        Field<NonNullGraphType<DateTimeGraphType>>("occurredAt").Resolve(c => c.Source.OccurredAt);
        Field<StringGraphType>("diffJson").Resolve(c => c.Source.DiffJson);
        Field<StringGraphType>("notes").Resolve(c => c.Source.Notes);
        Field<NonNullGraphType<ActorUnion>>("actor").Resolve(c =>
            new StaffActorDto { Id = Guid.Empty, DisplayName = c.Source.ActorUserId, Role = "User" });
    }
}
