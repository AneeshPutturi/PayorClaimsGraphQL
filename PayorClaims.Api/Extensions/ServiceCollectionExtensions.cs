using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using GraphQL;
using GraphQL.DataLoader;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PayorClaims.Api.Options;
using PayorClaims.Api.Security;
using PayorClaims.Application.Security;
using PayorClaims.Infrastructure.Export;
using PayorClaims.Infrastructure.Webhooks;
using PayorClaims.Schema.Loaders;
using PayorClaims.Schema.Schema;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PayorClaims.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? env = null)
    {
        // Options
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        services.Configure<GraphQLOptions>(configuration.GetSection(GraphQLOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddScoped<IActorContextProvider, ActorContextProvider>();

        // JWT Authentication
        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
        var key = Encoding.UTF8.GetBytes(authOptions.SigningKey.Length >= 32 ? authOptions.SigningKey : authOptions.SigningKey.PadRight(32));
        var isTesting = env?.EnvironmentName == "Testing";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                if (isTesting)
                    options.MapInboundClaims = false; // Keep "role", "sub", etc. so test tokens work
                options.RequireHttpsMetadata = authOptions.RequireHttps;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanReadMember", p => p.RequireRole("Admin", "Adjuster", "Provider", "Member"));
            options.AddPolicy("CanAdjudicate", p => p.RequireRole("Admin", "Adjuster"));
            options.AddPolicy("CanSubmitClaim", p => p.RequireRole("Admin", "Adjuster", "Provider"));
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
        });

        // CORS for Angular frontend
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        services.AddControllers();
        services.AddHttpClient();
        services.AddHostedService<WebhookDeliveryWorker>();
        services.AddHostedService<WebhookEventSubscriber>();
        services.AddHostedService<ExportJobWorker>();
        services.AddHostedService<PayorClaims.Infrastructure.Audit.AuditChainValidationJob>();

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

    public static IServiceCollection AddGraphQlServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AppQuery>();
        services.AddSingleton<AppMutation>();
        services.AddSingleton<AppSubscription>();
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

        var graphqlSection = configuration.GetSection(GraphQLOptions.SectionName);
        var maxDepth = graphqlSection.GetValue<int?>("MaxDepth") ?? 12;
        var maxComplexity = graphqlSection.GetValue<int?>("MaxComplexity") ?? 2000;

        services.AddGraphQL(b => b
            .AddSystemTextJson()
            .AddDataLoader()
            .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true)
            .AddComplexityAnalyzer(c =>
            {
                c.MaxDepth = maxDepth;
                c.MaxComplexity = maxComplexity;
                c.ValidateComplexityDelegate = async ctx =>
                {
                    if (ctx.Error != null)
                    {
                        ctx.Error.Code = ctx.Error.Message.Contains("depth", StringComparison.OrdinalIgnoreCase) ? "QUERY_TOO_DEEP" : "QUERY_TOO_COMPLEX";
                    }
                };
            }));

        var schemaAssembly = typeof(AppSchema).Assembly;
        foreach (var type in schemaAssembly.GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && type.Namespace?.StartsWith("PayorClaims.Schema") == true &&
                (typeof(global::GraphQL.Types.IGraphType).IsAssignableFrom(type)))
                services.AddSingleton(type);
        }
        return services;
    }
}
