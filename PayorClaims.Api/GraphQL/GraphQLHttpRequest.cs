using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayorClaims.Api.GraphQL;

public class GraphQLHttpRequest
{
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("operationName")]
    public string? OperationName { get; set; }

    [JsonPropertyName("variables")]
    public Dictionary<string, object?>? Variables { get; set; }

    [JsonPropertyName("extensions")]
    public Dictionary<string, object?>? Extensions { get; set; }

    /// <summary>
    /// Extracts extensions.persistedQuery.sha256Hash from the request (or from nested JSON).
    /// </summary>
    public static string? GetPersistedQueryHash(Dictionary<string, object?>? extensions)
    {
        if (extensions == null || !extensions.TryGetValue("persistedQuery", out var pq))
            return null;
        if (pq is JsonElement je)
        {
            if (je.TryGetProperty("sha256Hash", out var hashProp))
                return hashProp.GetString();
            return null;
        }
        if (pq is Dictionary<string, object?> dict && dict.TryGetValue("sha256Hash", out var hash))
            return hash?.ToString();
        return null;
    }
}
