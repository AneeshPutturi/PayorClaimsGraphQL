using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Scalars;
using PayorClaims.Schema.Types.Actors;
using PayorClaims.Schema.Unions;

namespace PayorClaims.Schema.Types;

public class HipaaAccessLogType : ObjectGraphType<HipaaAccessLog>
{
    public HipaaAccessLogType()
    {
        Name = "HipaaAccessLog";
        Field<NonNullGraphType<IdGraphType>>("accessLogId").Resolve(c => c.Source.AccessLogId);
        Field<NonNullGraphType<StringGraphType>>("actorType").Resolve(c => c.Source.ActorType);
        Field<IdGraphType>("actorId").Resolve(c => c.Source.ActorId);
        Field<NonNullGraphType<StringGraphType>>("action").Resolve(c => c.Source.Action);
        Field<NonNullGraphType<StringGraphType>>("subjectType").Resolve(c => c.Source.SubjectType);
        Field<NonNullGraphType<IdGraphType>>("subjectId").Resolve(c => c.Source.SubjectId);
        Field<NonNullGraphType<DateTimeGraphType>>("occurredAt").Resolve(c => c.Source.OccurredAt);
        Field<StringGraphType>("ipAddress").Resolve(c => c.Source.IpAddress);
        Field<StringGraphType>("userAgent").Resolve(c => c.Source.UserAgent);
        Field<NonNullGraphType<StringGraphType>>("purposeOfUse").Resolve(c => c.Source.PurposeOfUse);
        Field<NonNullGraphType<StringGraphType>>("prevHash").Resolve(c => c.Source.PrevHash);
        Field<NonNullGraphType<StringGraphType>>("hash").Resolve(c => c.Source.Hash);
        Field<NonNullGraphType<ActorUnion>>("actor").Resolve(c => ResolveActor(c.Source.ActorType, c.Source.ActorId));
    }

    private static object ResolveActor(string actorType, Guid? actorId)
    {
        var id = actorId ?? Guid.Empty;
        return actorType switch
        {
            "User" or "Staff" or "StaffUser" => new StaffActorDto { Id = id, DisplayName = "User", Role = "User" },
            "Provider" or "ProviderUser" => new ProviderActorDto { Id = id, Npi = "", Name = "Provider" },
            _ => new SystemActorDto { Id = id, Name = actorType }
        };
    }
}
