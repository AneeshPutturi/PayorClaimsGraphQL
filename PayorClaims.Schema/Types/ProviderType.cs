using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Scalars;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Schema.Types;

public class ProviderType : ObjectGraphType<Provider>
{
    public ProviderType()
    {
        Name = "Provider";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<StringGraphType>>("npi").Resolve(c => c.Source.Npi);
        Field<NonNullGraphType<StringGraphType>>("name").Resolve(c => c.Source.Name);
        Field<NonNullGraphType<StringGraphType>>("providerType").Resolve(c => c.Source.ProviderType);
        Field<StringGraphType>("specialty").Resolve(c => c.Source.Specialty);
        Field<StringGraphType>("taxId").Resolve(c => c.Source.TaxId);
        Field<NonNullGraphType<StringGraphType>>("providerStatus").Resolve(c => c.Source.ProviderStatus);
        Field<DateOnlyGraphType>("credentialedFrom").Resolve(c => c.Source.CredentialedFrom);
        Field<DateOnlyGraphType>("credentialedTo").Resolve(c => c.Source.CredentialedTo);
        Field<StringGraphType>("terminationReason").Resolve(c => c.Source.TerminationReason);

        Field<ListGraphType<NonNullGraphType<ProviderLocationType>>>("locations")
            .ResolveAsync(async c =>
            {
                var db = c.RequestServices!.GetRequiredService<ClaimsDbContext>();
                return await db.ProviderLocations.Where(l => l.ProviderId == c.Source.Id).ToListAsync();
            });
    }
}
