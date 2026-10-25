using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FleetGraph.Testing;

/// <summary>Minimal GraphQL-over-HTTP client for tests.</summary>
public static class GraphQLHttp
{
    public static async Task<GraphQLResponse> PostAsync(
        this HttpClient client,
        string query,
        object? variables = null,
        string? token = null,
        string path = "/graphql")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new { query, variables })
        };

        return await SendAsync(client, request, token);
    }

    /// <summary>Sends a trusted (persisted) document by id instead of the query text.</summary>
    public static async Task<GraphQLResponse> PostDocumentIdAsync(
        this HttpClient client,
        string documentId,
        object? variables = null,
        string? token = null,
        string path = "/graphql")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new { id = documentId, variables })
        };

        return await SendAsync(client, request, token);
    }

    private static async Task<GraphQLResponse> SendAsync(
        HttpClient client,
        HttpRequestMessage request,
        string? token)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/graphql-response+json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return new GraphQLResponse((int)response.StatusCode, body, JsonDocument.Parse(body));
    }
}

public sealed record GraphQLResponse(int StatusCode, string Body, JsonDocument Json)
{
    public JsonElement Root => Json.RootElement;

    public JsonElement Data => Root.GetProperty("data");

    public bool HasErrors => Root.TryGetProperty("errors", out var e) && e.GetArrayLength() > 0;

    public IReadOnlyList<string> ErrorCodes =>
        HasErrors
            ? Root.GetProperty("errors").EnumerateArray()
                .Select(e => e.TryGetProperty("extensions", out var x) && x.TryGetProperty("code", out var c)
                    ? c.GetString() ?? string.Empty
                    : string.Empty)
                .ToList()
            : [];

    /// <summary>Walks the <c>data</c> object: strings are property names, ints are list indexes.</summary>
    public JsonElement Get(params object[] path)
    {
        var current = Data;

        foreach (var segment in path)
        {
            current = segment switch
            {
                string name => current.GetProperty(name),
                int index => current[index],
                _ => throw new ArgumentException($"Unsupported path segment {segment}.")
            };
        }

        return current;
    }

    public override string ToString() => Body;
}
