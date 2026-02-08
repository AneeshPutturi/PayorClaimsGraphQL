using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Api.GraphQL;
using PayorClaims.Application.Abstractions;
using PayorClaims.Application.Security;
using PayorClaims.Schema.Schema;

namespace PayorClaims.Api.GraphQL;

public static class GraphQLHttpEndpoint
{
    public static async Task<IResult> HandlePostAsync(
        HttpContext http,
        AppSchema schema,
        IDocumentExecuter documentExecuter,
        IGraphQLTextSerializer serializer,
        IPersistedQueryStore persistedQueryStore,
        IConfiguration configuration,
        IActorContextProvider actorContextProvider)
    {
        var ct = http.RequestAborted;
        GraphQLHttpRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<GraphQLHttpRequest>(http.Request.Body, cancellationToken: ct);
        }
        catch
        {
            return Results.Json(new { errors = new[] { new { message = "Invalid JSON body", code = "INVALID_JSON" } } }, statusCode: 400);
        }

        if (request == null)
            return Results.Json(new { errors = new[] { new { message = "Request body required", code = "INVALID_JSON" } } }, statusCode: 400);

        var persistedEnabled = configuration.GetValue<bool>("GraphQL:PersistedQueriesEnabled");
        string? queryText;

        if (persistedEnabled)
        {
            if (!string.IsNullOrWhiteSpace(request.Query))
                return Results.Json(new { code = "PERSISTED_ONLY", message = "Persisted queries required" }, statusCode: 400);

            var hash = GraphQLHttpRequest.GetPersistedQueryHash(request.Extensions);
            if (string.IsNullOrWhiteSpace(hash))
                return Results.Json(new { code = "MISSING_HASH", message = "extensions.persistedQuery.sha256Hash required" }, statusCode: 400);

            queryText = await persistedQueryStore.GetQueryByHashAsync(hash, ct);
            if (queryText == null)
                return Results.Json(new { code = "UNKNOWN_HASH", message = "Unknown persisted query hash" }, statusCode: 400);
        }
        else
        {
            queryText = request.Query;
            if (string.IsNullOrWhiteSpace(queryText))
                return Results.Json(new { errors = new[] { new { message = "Query is required", code = "QUERY_REQUIRED" } } }, statusCode: 400);
        }

        // Ensure actor is set from request User (or from Bearer token when auth middleware did not run, e.g. in tests)
        var actor = BuildActorFromUser(http.User);
        var authHeader = http.Request.Headers.Authorization.FirstOrDefault();
        if (!actor.IsAuthenticated && !string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring(7).Trim();
            if (!string.IsNullOrEmpty(token))
                actor = BuildActorFromToken(token) ?? actor;
        }
        http.Items[ActorContext.HttpContextItemKey] = actor;

        var userContext = new Dictionary<string, object?>
        {
            [ActorContext.HttpContextItemKey] = actor
        };

        var opts = new ExecutionOptions
        {
            Schema = schema,
            Query = queryText,
            OperationName = request.OperationName,
            Variables = request.Variables != null ? new Inputs(ToInputsDict(request.Variables)) : null,
            RequestServices = http.RequestServices,
            User = http.User,
            UserContext = userContext,
            CancellationToken = ct
        };

        var result = await documentExecuter.ExecuteAsync(opts);

        http.Response.ContentType = "application/json; charset=utf-8";
        await serializer.WriteAsync(http.Response.Body, result, ct);
        return Results.Empty;
    }

    /// <summary>Build actor from JWT payload (no validation). Used when auth middleware did not run (e.g. test server).</summary>
    private static ActorContext? BuildActorFromToken(string bearerToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(bearerToken))
                return null;
            var jwt = handler.ReadJwtToken(bearerToken);
            var actor = new ActorContext();
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var actorId))
                actor.ActorId = actorId;
            var role = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(role))
                actor.Roles = role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            actor.Npi = jwt.Claims.FirstOrDefault(c => c.Type == "npi")?.Value;
            var memberIdStr = jwt.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (!string.IsNullOrEmpty(memberIdStr) && Guid.TryParse(memberIdStr, out var memberId))
                actor.MemberId = memberId;
            if (actor.Roles.Contains("Admin")) actor.ActorType = ActorContext.AdminActorType;
            else if (actor.Roles.Contains("Adjuster")) actor.ActorType = ActorContext.AdjusterActorType;
            else if (actor.Roles.Contains("Provider")) actor.ActorType = ActorContext.ProviderActorType;
            else if (actor.Roles.Contains("Member")) actor.ActorType = ActorContext.MemberActorType;
            return actor;
        }
        catch
        {
            return null;
        }
    }

    private static ActorContext BuildActorFromUser(ClaimsPrincipal? user)
    {
        var actor = new ActorContext();
        if (user?.Identity?.IsAuthenticated != true)
            return actor;
        var sub = user.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var actorId))
            actor.ActorId = actorId;
        var roles = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value;
        if (!string.IsNullOrEmpty(roles))
            actor.Roles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        actor.Npi = user.FindFirst("npi")?.Value;
        var memberIdStr = user.FindFirst("memberId")?.Value;
        if (!string.IsNullOrEmpty(memberIdStr) && Guid.TryParse(memberIdStr, out var memberId))
            actor.MemberId = memberId;
        if (actor.Roles.Contains("Admin")) actor.ActorType = ActorContext.AdminActorType;
        else if (actor.Roles.Contains("Adjuster")) actor.ActorType = ActorContext.AdjusterActorType;
        else if (actor.Roles.Contains("Provider")) actor.ActorType = ActorContext.ProviderActorType;
        else if (actor.Roles.Contains("Member")) actor.ActorType = ActorContext.MemberActorType;
        return actor;
    }

    private static Dictionary<string, object?> ToInputsDict(Dictionary<string, object?>? variables)
    {
        if (variables == null) return new Dictionary<string, object?>();
        var dict = new Dictionary<string, object?>();
        foreach (var kv in variables)
        {
            dict[kv.Key] = kv.Value is JsonElement je ? JsonElementToObject(je) : kv.Value;
        }
        return dict;
    }

    private static object? JsonElementToObject(JsonElement je)
    {
        return je.ValueKind switch
        {
            JsonValueKind.String => je.GetString(),
            JsonValueKind.Number => je.TryGetInt64(out var l) ? l : je.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => je.EnumerateArray().Select(JsonElementToObject).ToList(),
            JsonValueKind.Object => je.EnumerateObject().ToDictionary(p => p.Name, p => JsonElementToObject(p.Value)),
            _ => null
        };
    }
}
