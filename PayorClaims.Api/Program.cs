using System.Text.Json;
using GraphQL.Server.Ui.Altair;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PayorClaims.Api.Extensions;
using PayorClaims.Api.Middleware;
using PayorClaims.Application.Abstractions;
using PayorClaims.Application.Extensions;
using PayorClaims.Infrastructure;
using PayorClaims.Schema.Schema;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddGraphQlServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Exception handling + Problem Details
app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext ctx) =>
{
    var feature = ctx.Features.Get<IExceptionHandlerFeature>();
    var ex = feature?.Error;
    return Results.Problem(
        title: "Unhandled error",
        detail: app.Environment.IsDevelopment() ? ex?.ToString() : null,
        statusCode: StatusCodes.Status500InternalServerError,
        extensions: new Dictionary<string, object?>
        {
            ["traceId"] = ctx.TraceIdentifier
        });
});

app.UseStatusCodePages(async context =>
{
    var res = context.HttpContext.Response;
    if (res.HasStarted) return;
    await Results.Problem(
        title: "Request failed",
        statusCode: res.StatusCode,
        extensions: new Dictionary<string, object?>
        {
            ["traceId"] = context.HttpContext.TraceIdentifier
        }).ExecuteAsync(context.HttpContext);
});

// Correlation ID (early) then request logging
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diag, ctx) =>
    {
        if (ctx.Request.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var cid))
            diag.Set("CorrelationId", cid.ToString());
        diag.Set("TraceIdentifier", ctx.TraceIdentifier);
    };
});

app.UseRouting();
app.UseRateLimiter();

// Health checks (JSON with status and checks)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = WriteHealthResponseAsync
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponseAsync
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL<AppSchema>("/graphql").RequireRateLimiting("fixed");
});

app.UseGraphQLAltair("/ui/altair", new AltairOptions { GraphQLEndPoint = "/graphql" });

// Swagger only in Development under /admin
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => c.RouteTemplate = "admin/swagger/{documentName}/swagger.json");
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/admin/swagger/admin/swagger.json", "Admin v1");
        c.RoutePrefix = "admin/swagger";
    });
}

// Seed
var seedEnabled = builder.Configuration.GetValue<bool>("Seed:Enabled");
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (seedEnabled)
{
    logger.LogInformation("Seed enabled. Running migrations and seeding...");
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<ISeedRunner>();
    await seeder.RunAsync(app.Lifetime.ApplicationStopping);
}
else
{
    logger.LogInformation("Seed disabled. Skipping.");
}

app.Run();

static async Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var status = report.Status.ToString();
    var checks = report.Entries.Select(e => new
    {
        name = e.Key,
        status = e.Value.Status.ToString(),
        description = e.Value.Description
    });
    var payload = new { status, checks };
    await JsonSerializer.SerializeAsync(context.Response.Body, payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
}
