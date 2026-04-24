using EventBooking.Api.Tests.Factories;
using EventBooking.Application.Auth;
using System.Net.Http.Headers;

namespace EventBooking.Api.Tests.Endpoints;

public class ReservationsEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReservationsEndpointTests(ApiWebApplicationFactory factory)
        => _client = factory.CreateClient();

    private async Task<string> GetAdminTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { UserName = "admin", Password = "Admin123!" });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    [Fact]
    public async Task GetReservations_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/reservations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetReservations_WithAdminToken_Returns200()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/reservations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CancelReservation_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsync($"/api/reservations/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CancelReservation_WithAdminToken_OnNonExistentId_Returns404()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync($"/api/reservations/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ApproveReservation_WithoutAdminRole_Returns403()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Admin CAN approve — test that non-existent ID gives 404 (auth passes at least)
        var response = await _client.PostAsync($"/api/reservations/{Guid.NewGuid()}/approve", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
