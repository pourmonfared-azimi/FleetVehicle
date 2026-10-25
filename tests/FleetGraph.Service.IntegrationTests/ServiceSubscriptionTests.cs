using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FleetGraph.Service.IntegrationTests;

/// <summary>
/// The gateway consumes subgraph subscriptions over Server-Sent Events, so that is the
/// transport tested here.
/// </summary>
public class ServiceSubscriptionTests(ServiceFactory factory) : IClassFixture<ServiceFactory>
{
    [Fact]
    public async Task Status_changes_are_pushed_to_subscribers()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = JsonContent.Create(new
            {
                query = """
                    subscription {
                      serviceOrderStatusChanged {
                        previousStatus
                        newStatus
                        serviceOrder { id description status }
                        vehicle { id }
                      }
                    }
                    """
            })
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            timeout.Token);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var reader = new StreamReader(stream);

        // Give the subscription a moment to register its topic, then trigger an event.
        await Task.Delay(500, timeout.Token);

        await ChangeStatusAsync("IN_PROGRESS");
        var first = await ReadEventAsync(reader, timeout.Token);
        Assert.Contains("\"previousStatus\":\"SCHEDULED\"", first);
        Assert.Contains("\"newStatus\":\"IN_PROGRESS\"", first);
        Assert.Contains("\"description\":\"Tachograph calibration\"", first);
        Assert.Contains("\"status\":\"IN_PROGRESS\"", first);

        // A second event on the same long-lived subscription must not see cached data.
        await ChangeStatusAsync("AWAITING_PARTS");
        var second = await ReadEventAsync(reader, timeout.Token);
        Assert.Contains("\"previousStatus\":\"IN_PROGRESS\"", second);
        Assert.Contains("\"newStatus\":\"AWAITING_PARTS\"", second);
        Assert.Contains("\"status\":\"AWAITING_PARTS\"", second);
    }

    private async Task ChangeStatusAsync(string status)
    {
        var mutation = await factory.CreateClient().PostAsync(
            $$"""
            mutation {
              updateServiceOrderStatus(input: { serviceOrderId: 4, status: {{status}} }) {
                serviceOrder { status }
              }
            }
            """,
            token: TestTokens.Technician);
        Assert.False(mutation.HasErrors, mutation.Body);
    }

    private static async Task<string> ReadEventAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (line.StartsWith("data:", StringComparison.Ordinal) && line.Contains("serviceOrderStatusChanged"))
            {
                return line["data:".Length..].Trim();
            }
        }

        throw new InvalidOperationException("The event stream ended before an event arrived.");
    }
}
