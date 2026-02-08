using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;
using PayorClaims.Schema.Loaders;
using PayorClaims.Schema.Scalars;
using PayorClaims.Schema.Types.Actors;
namespace PayorClaims.Schema.Types;

public class ClaimType : ObjectGraphType<Claim>
{
    public ClaimType()
    {
        Name = "Claim";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<StringGraphType>>("claimNumber").Resolve(c => c.Source.ClaimNumber);
        Field<NonNullGraphType<IdGraphType>>("memberId").Resolve(c => c.Source.MemberId);
        Field<NonNullGraphType<IdGraphType>>("providerId").Resolve(c => c.Source.ProviderId);
        Field<IdGraphType>("coverageId").Resolve(c => c.Source.CoverageId);
        Field<NonNullGraphType<DateOnlyGraphType>>("receivedDate").Resolve(c => c.Source.ReceivedDate);
        Field<NonNullGraphType<DateOnlyGraphType>>("serviceFromDate").Resolve(c => c.Source.ServiceFromDate);
        Field<NonNullGraphType<DateOnlyGraphType>>("serviceToDate").Resolve(c => c.Source.ServiceToDate);
        Field<NonNullGraphType<StringGraphType>>("status").Resolve(c => c.Source.Status);
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("totalBilled").Resolve(c => c.Source.TotalBilled);
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("totalAllowed").Resolve(c => c.Source.TotalAllowed);
        Field<NonNullGraphType<PayorClaims.Schema.Scalars.DecimalGraphType>>("totalPaid").Resolve(c => c.Source.TotalPaid);
        Field<NonNullGraphType<StringGraphType>>("currency").Resolve(c => c.Source.Currency);
        Field<StringGraphType>("idempotencyKey").Resolve(c => c.Source.IdempotencyKey);
        Field<StringGraphType>("rowVersion").Resolve(c => c.Source.RowVersion != null && c.Source.RowVersion.Length > 0 ? Convert.ToBase64String(c.Source.RowVersion) : null);
        Field<StringGraphType>("sourceSystem").Resolve(c => c.Source.SourceSystem);
        Field<NonNullGraphType<StringGraphType>>("submittedByActorType").Resolve(c => c.Source.SubmittedByActorType);
        Field<IdGraphType>("submittedByActorId").Resolve(c => c.Source.SubmittedByActorId);

        Field<ListGraphType<NonNullGraphType<ClaimLineType>>>("lines")
            .Resolve(c =>
            {
                var accessor = c.RequestServices!.GetRequiredService<IDataLoaderContextAccessor>();
                var loader = c.RequestServices!.GetRequiredService<ClaimLinesByClaimIdLoader>();
                return loader.LoadAsync(c.Source.Id, accessor.Context!);
            });

        Field<ListGraphType<NonNullGraphType<ClaimDiagnosisType>>>("diagnoses")
            .Resolve(c =>
            {
                var accessor = c.RequestServices!.GetRequiredService<IDataLoaderContextAccessor>();
                var loader = c.RequestServices!.GetRequiredService<DiagnosesByClaimIdLoader>();
                return loader.LoadAsync(c.Source.Id, accessor.Context!);
            });

        Field<ProviderType>("provider")
            .Resolve(c =>
            {
                var accessor = c.RequestServices!.GetRequiredService<IDataLoaderContextAccessor>();
                var loader = c.RequestServices!.GetRequiredService<ProviderByIdLoader>();
                return loader.LoadAsync(c.Source.ProviderId, accessor.Context!);
            });
    }
}
