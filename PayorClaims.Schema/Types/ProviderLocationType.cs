using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;

namespace PayorClaims.Schema.Types;

public class ProviderLocationType : ObjectGraphType<ProviderLocation>
{
    public ProviderLocationType()
    {
        Name = "ProviderLocation";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("providerId").Resolve(c => c.Source.ProviderId);
        Field<NonNullGraphType<StringGraphType>>("addressLine1").Resolve(c => c.Source.AddressLine1);
        Field<StringGraphType>("addressLine2").Resolve(c => c.Source.AddressLine2);
        Field<NonNullGraphType<StringGraphType>>("city").Resolve(c => c.Source.City);
        Field<NonNullGraphType<StringGraphType>>("state").Resolve(c => c.Source.State);
        Field<NonNullGraphType<StringGraphType>>("zip").Resolve(c => c.Source.Zip);
        Field<StringGraphType>("phone").Resolve(c => c.Source.Phone);
        Field<NonNullGraphType<BooleanGraphType>>("isPrimary").Resolve(c => c.Source.IsPrimary);
    }
}
