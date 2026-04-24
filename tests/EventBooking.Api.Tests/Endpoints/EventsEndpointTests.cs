using EventBooking.Api.Tests.Factories;
using EventBooking.Application.Common;
using EventBooking.Application.Events.Queries.GetEvents;

namespace EventBooking.Api.Tests.Endpoints;

public class EventsEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventsEndpointTests(ApiWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GetEvents_WithoutAuth_Returns200()
    {
        var response = await _client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEvents_ReturnsPagedResult()
    {
        var response = await _client.GetAsync("/api/events");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetEventsResponse>>();

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.PageNumber >= 1);
        Assert.True(result.PageSize >= 1);
    }

    [Fact]
    public async Task GetEventById_WithNonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/events/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            Title = "Test",
            LocationId = 1,
            BasePrice = 100,
            Capacity = 50
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
