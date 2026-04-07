using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ApplicationTracker.IntegrationTests;

public class AuthTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;

    public AuthTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            email = $"test_{Guid.NewGuid()}@example.com",
            password = "Password123!",
            name = "Test User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup_{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "Password123!",
            name = "User One"
        });

        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "Password123!",
            name = "User Two"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var email = $"login_{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "Password123!",
            name = "Login User"
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"wrong_{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "Password123!",
            name = "Wrong Password User"
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDashboard_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Health_Returns200()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private record LoginResponse(string AccessToken, string RefreshToken, string Email, string Name);
}
