using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Application.Abstractions;
using PayorClaims.Infrastructure.Persistence;
using PayorClaims.Infrastructure.Seed;

namespace PayorClaims.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ClaimsDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("ClaimsDb")));

        services.AddHealthChecks()
            .AddDbContextCheck<ClaimsDbContext>("db", tags: new[] { "ready" });

        services.AddScoped<ISeedRunner, SeedRunner>();

        return services;
    }
}
