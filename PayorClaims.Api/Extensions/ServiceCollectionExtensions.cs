using System.Reflection;
using System.Threading.RateLimiting;
using GraphQL;
using GraphQL.DataLoader;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using PayorClaims.Api.Options;
using PayorClaims.Schema.Loaders;
using PayorClaims.Schema.Schema;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PayorClaims.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

        // Problem Details
        services.AddProblemDetails();

        // Health checks (live only; ready/db added by AddInfrastructure when DbContext is registered)
        services.AddHealthChecks()
            .AddCheck("live", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

        // Rate limiting
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = 60;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
        });

        // Swagger (admin only)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("admin", new OpenApiInfo { Title = "PayorClaims Admin", Version = "v1" });
        });

        return services;
    }

    public static IServiceCollection AddGraphQlServices(this IServiceCollection services)
    {
        services.AddSingleton<AppQuery>();
        services.AddSingleton<AppMutation>();
        services.AddSingleton<AppSchema>();
        services.AddSingleton<IDataLoaderContextAccessor, DataLoaderContextAccessor>();

        // Rule: DbContext is scoped. Loaders that need it must be Scoped; resolve via context.RequestServices in resolvers.
        services.AddScoped<ClaimsByMemberIdLoader>();
        services.AddScoped<ClaimLinesByClaimIdLoader>();
        services.AddScoped<DiagnosesByClaimIdLoader>();
        services.AddScoped<ProviderByIdLoader>();
        services.AddScoped<CoveragesByMemberIdLoader>();
        services.AddScoped<PlanByIdLoader>();
        services.AddScoped<EffectiveBenefitsByPlanIdLoader>();

        services.AddGraphQL(b => b
            .AddSystemTextJson()
            .AddDataLoader()
            .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true));

        var schemaAssembly = typeof(AppSchema).Assembly;
        foreach (var type in schemaAssembly.GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && type.Namespace?.StartsWith("PayorClaims.Schema") == true &&
                (typeof(GraphQL.Types.IGraphType).IsAssignableFrom(type)))
                services.AddSingleton(type);
        }
        return services;
    }
}
