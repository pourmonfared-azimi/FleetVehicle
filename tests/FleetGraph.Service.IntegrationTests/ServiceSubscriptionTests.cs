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
    public async Task Status_change_is_pushed_to_subscribers()
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
                        serviceOrder { id description }
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
        var mutation = await factory.CreateClient().PostAsync(
            """
            mutation {
              updateServiceOrderStatus(input: { serviceOrderId: 4, status: IN_PROGRESS }) {
                serviceOrder { status }
              }
            }
            """,
            token: TestTokens.Technician);
        Assert.False(mutation.HasErrors, mutation.Body);

        string? data = null;
        while (!timeout.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(timeout.Token);
            if (line is null)
            {
                break;
            }

            if (line.StartsWith("data:", StringComparison.Ordinal) && line.Contains("serviceOrderStatusChanged"))
            {
                data = line["data:".Length..].Trim();
                break;
            }
        }

        Assert.NotNull(data);
        Assert.Contains("\"previousStatus\":\"SCHEDULED\"", data);
        Assert.Contains("\"newStatus\":\"IN_PROGRESS\"", data);
        Assert.Contains("\"description\":\"Tachograph calibration\"", data);
    }
}
