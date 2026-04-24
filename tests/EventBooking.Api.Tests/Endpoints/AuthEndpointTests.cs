using EventBooking.Api.Tests.Factories;
using EventBooking.Application.Auth;

namespace EventBooking.Api.Tests.Endpoints;

public class AuthEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(ApiWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithValidAdminCredentials_Returns200AndToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { UserName = "admin", Password = "Admin123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.Token);
        Assert.Equal("admin", body.UserName);
        Assert.Equal("Admin", body.Role);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { UserName = "admin", Password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUser_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { UserName = "nobody", Password = "any" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
