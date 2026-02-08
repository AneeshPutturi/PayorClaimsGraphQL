using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Application.Abstractions;
using PayorClaims.Infrastructure.Options;
using PayorClaims.Infrastructure.Persistence;
using PayorClaims.Infrastructure.Seed;
using PayorClaims.Infrastructure.Services;

namespace PayorClaims.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<LocalStorageOptions>(config.GetSection(LocalStorageOptions.SectionName));

        services.AddDbContext<ClaimsDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("ClaimsDb")));

        services.AddHealthChecks()
            .AddDbContextCheck<ClaimsDbContext>("db", tags: new[] { "ready" });

        services.AddScoped<ISeedRunner, SeedRunner>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IAccessLogger, AccessLogger>();
        services.AddScoped<IClaimAttachmentService, ClaimAttachmentService>();

        return services;
    }
}
