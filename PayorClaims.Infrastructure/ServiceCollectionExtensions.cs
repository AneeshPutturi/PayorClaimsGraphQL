using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PayorClaims.Application.Abstractions;
using PayorClaims.Infrastructure.Options;
using PayorClaims.Infrastructure.Persistence;
using PayorClaims.Infrastructure.Seed;
using PayorClaims.Infrastructure.Services;

namespace PayorClaims.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, IHostEnvironment? env = null)
    {
        services.Configure<LocalStorageOptions>(o => config.GetSection(LocalStorageOptions.SectionName).Bind(o));

        if (env?.EnvironmentName == "Testing")
        {
            // Shared in-memory DB: each DbContext gets its own connection (avoids concurrent-use corruption).
            const string testConn = "Data Source=payorclaims_test;Mode=Memory;Cache=Shared";
            var keeper = new SqliteConnection(testConn);
            keeper.Open();
            services.AddSingleton(keeper);
            services.AddDbContext<ClaimsDbContext>(opt => opt.UseSqlite(testConn));
        }
        else
        {
            services.AddDbContext<ClaimsDbContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("ClaimsDb")));
        }

        services.AddHealthChecks()
            .AddDbContextCheck<ClaimsDbContext>("db", tags: new[] { "ready" });

        services.AddScoped<ISeedRunner, SeedRunner>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IAccessLogger, AccessLogger>();
        services.AddScoped<IClaimAttachmentService, ClaimAttachmentService>();
        services.AddScoped<PayorClaims.Application.Security.IConsentService, ConsentService>();
        services.AddSingleton<PayorClaims.Application.Events.IEventBus, PayorClaims.Infrastructure.Events.InMemoryEventBus>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IExportService, ExportService>();
        services.Configure<PersistedQueries.FilePersistedQueryStoreOptions>(o => o.FilePath = config.GetValue<string>("GraphQL:PersistedQueriesPath") ?? "persisted-queries.json");
        services.AddSingleton<PayorClaims.Application.Abstractions.IPersistedQueryStore, PersistedQueries.FilePersistedQueryStore>();

        return services;
    }
}
