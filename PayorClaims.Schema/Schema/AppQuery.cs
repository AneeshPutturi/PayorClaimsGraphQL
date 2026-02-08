using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Application.Abstractions;
using PayorClaims.Application.Security;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;
using PayorClaims.Schema;
using PayorClaims.Schema.Inputs;
using PayorClaims.Schema.Types;

namespace PayorClaims.Schema.Schema;

// Normal GraphQL queries and loaders do NOT use IgnoreQueryFilters(); soft-delete filters apply.

/*
Example Member360 query:
query {
  memberById(id:"...") {
    id
    firstName
    activeCoverage(asOf:"2026-02-07") {
      id
      plan { planCode name effectiveBenefits(asOf:"2026-02-07"){ category network copayAmount } }
    }
    recentClaims(limit:5){
      claimNumber status
      provider { npi name }
      lines { lineNumber cptCode billedAmount lineStatus }
      diagnoses { codeSystem code isPrimary lineNumber }
    }
  }
}
*/
public class AppQuery : ObjectGraphType
{
    public AppQuery()
    {
        Field<StringGraphType>("ping").Resolve(_ => "pong");

        Field<MemberType>("memberById")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .ResolveAsync(async c =>
            {
                var db = c.RequestServices!.GetRequiredService<ClaimsDbContext>();
                var idStr = c.GetArgument<string>("id");
                if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var id))
                    return null;
                var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, c.CancellationToken);
                if (member != null)
                {
                    var accessLogger = c.RequestServices!.GetRequiredService<IAccessLogger>();
                    await accessLogger.LogReadAsync("System", null, "Member", id, "GraphQL query", c.CancellationToken);
                }
                return member;
            });

        Field<ClaimType>("claimById")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .ResolveAsync(async c =>
            {
                var db = c.RequestServices!.GetRequiredService<ClaimsDbContext>();
                var idStr = c.GetArgument<string>("id");
                if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var id))
                    return null;
                var claim = await db.Claims.AsNoTracking().FirstOrDefaultAsync(cl => cl.Id == id, c.CancellationToken);
                if (claim != null)
                {
                    var accessLogger = c.RequestServices!.GetRequiredService<IAccessLogger>();
                    await accessLogger.LogReadAsync("System", null, "Claim", id, "GraphQL query", c.CancellationToken);
                }
                return claim;
            });

        Field<EobType>("eobById")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .ResolveAsync(async c =>
            {
                var db = c.RequestServices!.GetRequiredService<ClaimsDbContext>();
                var idStr = c.GetArgument<string>("id");
                if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var id))
                    return null;
                var eob = await db.Eobs.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, c.CancellationToken);
                if (eob != null)
                {
                    var accessLogger = c.RequestServices!.GetRequiredService<IAccessLogger>();
                    await accessLogger.LogReadAsync("System", null, "Eob", id, "GraphQL query", c.CancellationToken);
                }
                return eob;
            });

        Field<ProviderType>("providerByNpi")
            .Argument<NonNullGraphType<StringGraphType>>("npi")
            .ResolveAsync(async c =>
            {
                var db = c.RequestServices!.GetRequiredService<ClaimsDbContext>();
                var npi = c.GetArgument<string>("npi")!;
                return await db.Providers.AsNoTracking().FirstOrDefaultAsync(p => p.Npi == npi, c.CancellationToken);
            });

        Field<NonNullGraphType<MemberPageType>>("members")
            .Argument<MemberFilterInputType>("filter")
            .Argument<PageInputType>("page")
            .Argument<MemberSortFieldEnum>("sortField")
            .Argument<SortDirectionEnum>("sortDir")
            .ResolveAsync(async c =>
            {
                var db = c.RequestServices!.GetRequiredService<ClaimsDbContext>();
                var filter = c.GetArgument<MemberFilterInput?>("filter");
                var page = c.GetArgument<PageInput?>("page") ?? new PageInput();
                var sortField = c.GetArgument("sortField", MemberSortField.LastName);
                var sortDir = c.GetArgument("sortDir", SortDirection.ASC);
                var take = Math.Clamp(page.Take, 0, 100);
                var skip = Math.Max(0, page.Skip);

                var query = db.Members.AsNoTracking();
                if (filter != null)
                {
                    if (!string.IsNullOrEmpty(filter.Status))
                        query = query.Where(m => m.Status == filter.Status);
                    if (!string.IsNullOrEmpty(filter.NameContains))
                    {
                        var term = filter.NameContains.ToLower();
                        query = query.Where(m => (m.FirstName + " " + m.LastName).ToLower().Contains(term));
                    }
                    if (filter.DobFrom.HasValue)
                        query = query.Where(m => m.Dob >= filter.DobFrom!.Value);
                    if (filter.DobTo.HasValue)
                        query = query.Where(m => m.Dob <= filter.DobTo!.Value);
                }

                var totalCount = await query.CountAsync(c.CancellationToken);
                query = ApplySort(query, sortField, sortDir);
                var items = await query.Skip(skip).Take(take).ToListAsync(c.CancellationToken);
                return new MemberPage { Items = items, TotalCount = totalCount, Skip = skip, Take = take };
            });

        Field<NonNullGraphType<ClaimPageType>>("claims")
            .Argument<ClaimFilterInputType>("filter")
            .Argument<PageInputType>("page")
            .Argument<ClaimSortFieldEnum>("sortField")
            .Argument<SortDirectionEnum>("sortDir")
            .ResolveAsync(async c =>
            {
                var db = c.RequestServices!.GetRequiredService<ClaimsDbContext>();
                var filter = c.GetArgument<ClaimFilterInput?>("filter");
                var page = c.GetArgument<PageInput?>("page") ?? new PageInput();
                var sortField = c.GetArgument("sortField", ClaimSortField.ReceivedDate);
                var sortDir = c.GetArgument("sortDir", SortDirection.DESC);
                var take = Math.Clamp(page.Take, 0, 100);
                var skip = Math.Max(0, page.Skip);

                var query = db.Claims.AsNoTracking();
                if (filter != null)
                {
                    if (filter.MemberId.HasValue)
                        query = query.Where(x => x.MemberId == filter.MemberId!.Value);
                    if (filter.ProviderId.HasValue)
                        query = query.Where(x => x.ProviderId == filter.ProviderId!.Value);
                    if (!string.IsNullOrEmpty(filter.Status))
                        query = query.Where(x => x.Status == filter.Status);
                    if (filter.ReceivedFrom.HasValue)
                        query = query.Where(x => x.ReceivedDate >= filter.ReceivedFrom!.Value);
                    if (filter.ReceivedTo.HasValue)
                        query = query.Where(x => x.ReceivedDate <= filter.ReceivedTo!.Value);
                    if (!string.IsNullOrEmpty(filter.ClaimNumber))
                        query = query.Where(x => x.ClaimNumber.Contains(filter.ClaimNumber));
                }

                var totalCount = await query.CountAsync(c.CancellationToken);
                query = ApplySort(query, sortField, sortDir);
                var items = await query.Skip(skip).Take(take).ToListAsync(c.CancellationToken);
                return new ClaimPage { Items = items, TotalCount = totalCount, Skip = skip, Take = take };
            });

        Field<NonNullGraphType<ProviderPageType>>("providers")
            .Argument<ProviderFilterInputType>("filter")
            .Argument<PageInputType>("page")
            .Argument<ProviderSortFieldEnum>("sortField")
            .Argument<SortDirectionEnum>("sortDir")
            .ResolveAsync(async c =>
            {
                var db = c.RequestServices!.GetRequiredService<ClaimsDbContext>();
                var filter = c.GetArgument<ProviderFilterInput?>("filter");
                var page = c.GetArgument<PageInput?>("page") ?? new PageInput();
                var sortField = c.GetArgument("sortField", ProviderSortField.Name);
                var sortDir = c.GetArgument("sortDir", SortDirection.ASC);
                var take = Math.Clamp(page.Take, 0, 100);
                var skip = Math.Max(0, page.Skip);

                var query = db.Providers.AsNoTracking();
                if (filter != null)
                {
                    if (!string.IsNullOrEmpty(filter.Npi))
                        query = query.Where(p => p.Npi == filter.Npi);
                    if (!string.IsNullOrEmpty(filter.NameContains))
                    {
                        var term = filter.NameContains.ToLower();
                        query = query.Where(p => p.Name.ToLower().Contains(term));
                    }
                    if (!string.IsNullOrEmpty(filter.Specialty))
                        query = query.Where(p => p.Specialty != null && p.Specialty == filter.Specialty);
                    if (!string.IsNullOrEmpty(filter.Status))
                        query = query.Where(p => p.ProviderStatus == filter.Status);
                }

                var totalCount = await query.CountAsync(c.CancellationToken);
                query = ApplySort(query, sortField, sortDir);
                var items = await query.Skip(skip).Take(take).ToListAsync(c.CancellationToken);
                return new ProviderPage { Items = items, TotalCount = totalCount, Skip = skip, Take = take };
            });

        Field<ExportJobStatusResultType>("exportJob")
            .Argument<NonNullGraphType<IdGraphType>>("jobId")
            .ResolveAsync(async c =>
            {
                var jobId = c.GetArgument<Guid>("jobId");
                var actorProvider = c.RequestServices!.GetRequiredService<IActorContextProvider>();
                var actor = actorProvider.GetActorContext();
                var service = c.RequestServices!.GetRequiredService<IExportService>();
                return await service.GetExportJobStatusAsync(jobId, actor, c.CancellationToken);
            });
    }

    private static IQueryable<Member> ApplySort(IQueryable<Member> query, MemberSortField field, SortDirection dir)
    {
        return field switch
        {
            MemberSortField.LastName => dir == SortDirection.ASC ? query.OrderBy(m => m.LastName) : query.OrderByDescending(m => m.LastName),
            MemberSortField.FirstName => dir == SortDirection.ASC ? query.OrderBy(m => m.FirstName) : query.OrderByDescending(m => m.FirstName),
            MemberSortField.Dob => dir == SortDirection.ASC ? query.OrderBy(m => m.Dob) : query.OrderByDescending(m => m.Dob),
            _ => dir == SortDirection.ASC ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt)
        };
    }

    private static IQueryable<Claim> ApplySort(IQueryable<Claim> query, ClaimSortField field, SortDirection dir)
    {
        return field switch
        {
            ClaimSortField.ReceivedDate => dir == SortDirection.ASC ? query.OrderBy(x => x.ReceivedDate) : query.OrderByDescending(x => x.ReceivedDate),
            ClaimSortField.Status => dir == SortDirection.ASC ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
            ClaimSortField.TotalBilled => dir == SortDirection.ASC ? query.OrderBy(x => x.TotalBilled) : query.OrderByDescending(x => x.TotalBilled),
            _ => dir == SortDirection.ASC ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)
        };
    }

    private static IQueryable<Provider> ApplySort(IQueryable<Provider> query, ProviderSortField field, SortDirection dir)
    {
        return field switch
        {
            ProviderSortField.Name => dir == SortDirection.ASC ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            ProviderSortField.Npi => dir == SortDirection.ASC ? query.OrderBy(p => p.Npi) : query.OrderByDescending(p => p.Npi),
            ProviderSortField.Specialty => dir == SortDirection.ASC ? query.OrderBy(p => p.Specialty ?? "") : query.OrderByDescending(p => p.Specialty ?? ""),
            _ => dir == SortDirection.ASC ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
        };
    }
}
